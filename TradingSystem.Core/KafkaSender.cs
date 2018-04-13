using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace TradingSystem.Core
{
    class KafkaSender
    {
        Dictionary<string, object> _config;
        BlockingCollection<Message> _outputs = new BlockingCollection<Message>();
        Producer<Null, string> _producer;
        Thread _producerLoop;
        bool _canceled;

        public KafkaSender(string brokerList,
                              string topic)
        {
            _config = new Dictionary<string, object>
            {
                { "group.id", Guid.NewGuid().ToString() },
                { "enable.auto.commit", false },
                { "bootstrap.servers", brokerList },
            };

            _producer = new Producer<Null, string>(_config, null, new StringSerializer(Encoding.UTF8));

            _producerLoop = new Thread(() => ProducerLoop(topic));
        }

        public void Connect()
        {
            if (_producerLoop.ThreadState == ThreadState.Unstarted)
            {
                _producerLoop.Start();
            }
            else
            {
                throw new InvalidOperationException("Already started");
            }
        }

        public void Disconnect()
        {
            _canceled = true;
            lock (_outputs)
            {
                Console.WriteLine("Waiting for disconnection");
                _outputs.CompleteAdding();
                _producerLoop.Join();
                _producer.Dispose();
                Console.WriteLine("Disconnected");
            }
        }

        void ProducerLoop(string topic)
        {
            foreach (var item in _outputs.GetConsumingEnumerable())
            {
                var text = JsonConvert.SerializeObject(
                                item, 
                                Newtonsoft.Json.Formatting.None,
                                new JsonSerializerSettings
                                {
                                    NullValueHandling = NullValueHandling.Ignore
                                });
                _producer.ProduceAsync(topic, null, text).Wait();
            }
        }

        public void Send(Message item)
        {
            lock (_outputs)
                if (!_canceled)
                    _outputs.Add(item);
        }
    }
}