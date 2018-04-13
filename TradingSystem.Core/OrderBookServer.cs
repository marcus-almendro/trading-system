using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Grpc.Core;

namespace TradingSystem.Core
{
    public class OrderBookServer : Service.ServiceBase
    {
        string _brokerIds;
        OrderBook _orderBook;
        KafkaReceiver _marketDataReceiver;
        KafkaSender _marketDataSender;
        Grpc.Core.Server _server;

        public OrderBookServer(string host, int port, string brokerIds)
        {
            _brokerIds = brokerIds;
            _orderBook = new OrderBook();
            _marketDataSender = new KafkaSender(brokerIds, "MarketData");
            _marketDataReceiver = new KafkaReceiver(brokerIds, "MarketData", true);
            _server = new Grpc.Core.Server
            {
                Services = { Service.BindService(this) },
                Ports = { { host, port, ServerCredentials.Insecure } }
            };

        }

        public void TakeSnapshot()
        {
            lock (_orderBook)
            {
                var snapshotSender = new KafkaSender(_brokerIds, "MarketData.New");
                snapshotSender.Connect();
                _orderBook.Dump().ForEach(o => snapshotSender.Send(new Message(o)));
                snapshotSender.Disconnect();
            }
        }

        void GetSnapshot()
        {
            _marketDataReceiver.OnReceivedInput += (s, m) =>
                HandleMessage(m, result =>
                    {
                        if (result != ErrorCode.Success)
                            throw new InvalidOperationException($"Not Successful Snapshot Sync: {result.Description}");
                    });

            Console.WriteLine("Syncing Snapshot...");
            using (var wait = new ManualResetEvent(false))
            {
                _marketDataReceiver.Connect();
                _marketDataReceiver.InnerConsumer().OnPartitionEOF += (_, end) => wait.Set();
                wait.WaitOne();
            }

            _marketDataReceiver.Disconnect();
            _marketDataReceiver.Dispose();
            Console.WriteLine($"Snapshot Synced. Order Count: {_orderBook.Count}, TradeId: {_orderBook.TradeId}");
        }

        public void Connect()
        {
            GetSnapshot();

            _orderBook.OnMessage += (s, m) => _marketDataSender.Send(m);
            _marketDataSender.Connect();

            _server.Start();
        }

        public override Task<ErrorCodeMsg> EnterOrder(OrderMsg order, ServerCallContext context)
        {
            try
            {
                lock (_orderBook)
                {
                    return Task.FromResult(_orderBook.Enter(order.ToOrder()).ToErrorCodeMsg());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        void HandleMessage(Message m, Action<ErrorCode> resultAction)
        {
            if (m.Type == MessageType.Order)
            {
                lock (_orderBook)
                {
                    var result = _orderBook.Enter(m.Order);
                    resultAction(result);
                }
            }
            else
            {
                _orderBook.TradeId = m.Trade.Id;
            }
        }

    }
}