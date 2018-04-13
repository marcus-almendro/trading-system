using System;
using Grpc.Core;

namespace TradingSystem.Core
{
    public class OrderBookClient : IOrderBook
    {
        public event EventHandler<Message> OnMessage;
        Service.ServiceClient _client;
        KafkaReceiver _marketDataReceiver;

        public OrderBookClient(string serverHost, int serverPort, string brokerIds)
        {
            _marketDataReceiver = new KafkaReceiver(brokerIds, "MarketData", true);
            _marketDataReceiver.OnReceivedInput += (s, t) => OnMessage?.Invoke(this, t);

            _client = new Service.ServiceClient(new Channel(serverHost, serverPort, ChannelCredentials.Insecure));
        }

        public void Connect()
        {
            _marketDataReceiver.Connect();
        }

        public void Disconnect()
        {
            _marketDataReceiver.Disconnect();
        }

        public ErrorCode Enter(Order order)
        {
            return _client.EnterOrder(order.ToOrderMsg()).ToErrorCode();
        }

    }
}