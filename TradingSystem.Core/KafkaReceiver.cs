using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace TradingSystem.Core
{
    class KafkaReceiver
    {
        public event EventHandler<Message> OnReceivedInput;
        Dictionary<string, object> _config;
        BlockingCollection<Message> _inputs = new BlockingCollection<Message>();
        Consumer<Null, string> _consumer;
        ManualResetEventSlim _consumerReady = new ManualResetEventSlim(false);
        Thread _consumerLoop, _consumerQueueLoop;
        bool _canceled;

        public KafkaReceiver(string brokerList,
                              string topic,
                              bool fromBeginning)
        {
            var position = fromBeginning ? "smallest" : "largest";
            _config = new Dictionary<string, object>
            {
                { "group.id", Guid.NewGuid().ToString() },
                { "enable.auto.commit", false },
                { "bootstrap.servers", brokerList },
                { "default.topic.config", new Dictionary<string, object>()
                    {
                        { "auto.offset.reset", position }
                    }
                }
            };

            _consumer = new Consumer<Null, string>(_config, null, new StringDeserializer(Encoding.UTF8));

            _consumer.OnError += (_, error)
                => Console.WriteLine($"Error: {error}");

            _consumer.OnConsumeError += (_, error)
                => Console.WriteLine($"Consume error: {error.Error}");

            _consumer.OnPartitionsAssigned += (_, partitions) =>
            {
                Console.WriteLine($"Assigned partitions: [{string.Join(", ", partitions)}], member id: {_consumer.MemberId}");
                _consumer.Assign(partitions);
                _consumerReady.Set();
            };

            _consumer.OnPartitionsRevoked += (_, partitions) =>
            {
                Console.WriteLine($"Revoked partitions: [{string.Join(", ", partitions)}]");
                _consumer.Unassign();
            };

            _consumerLoop = new Thread(() => ConsumerLoop(topic));
            _consumerQueueLoop = new Thread(() => ConsumerQueueLoop());
        }

        public void Connect()
        {
            if (_consumerLoop.ThreadState == ThreadState.Unstarted)
            {
                _consumerLoop.Start();
                _consumerQueueLoop.Start();

                Console.WriteLine("Waiting for assignment...");
                _consumerReady.Wait();
            }
            else
            {
                throw new InvalidOperationException("Already started");
            }
        }

        public void Disconnect()
        {
            _canceled = true;
            Console.WriteLine("Waiting for disconnection");
            _consumerLoop.Join();
            _inputs.CompleteAdding();
            _consumerQueueLoop.Join();
            Console.WriteLine("Disconnected");
        }

        void ConsumerLoop(string topic)
        {
            _consumer.Subscribe(topic);

            while (!_canceled)
            {
                Message<Null, string> msg;
                if (!_consumer.Consume(out msg, TimeSpan.FromMilliseconds(100)))
                {
                    continue;
                }

                var item = JsonConvert.DeserializeObject<Message>(msg.Value);
                _inputs.Add(item);
            }
            Console.WriteLine("Exited ConsumerLoop");
        }

        void ConsumerQueueLoop()
        {
            foreach (var item in _inputs.GetConsumingEnumerable())
            {
                OnReceivedInput?.Invoke(this, item);
            }
            Console.WriteLine("Exited ConsumerQueueLoop");
        }

        public void Dispose()
        {
            _consumer.Dispose();
            _consumerReady.Dispose();
            _inputs.Dispose();
            _consumerQueueLoop = null;
            _consumerLoop = null;
            _config = null;
            _consumer = null;
            _inputs = null;
        }

        internal Consumer<Null, string> InnerConsumer()
        {
            return _consumer;
        }
    }
}