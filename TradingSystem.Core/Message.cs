using System;
using Newtonsoft.Json;

namespace TradingSystem.Core
{
    public class Message
    {
        public MessageType Type { get; set; }
        public Order Order { get; set; }
        public Trade Trade { get; set; }
        public ErrorCode ErrorCode { get; set; }
        
        public Message()
        {
            
        }

        public Message(Order order)
        {
            Order = order;
            Type = MessageType.Order;
        }

        public Message(Trade trade)
        {
            Trade = trade;
            Type = MessageType.Trade;
        }
    }
}