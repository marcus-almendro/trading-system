using System.Collections.Generic;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Orders;

namespace TradingSystem.Domain.Events.Orders
{
    public class OrderEvent : DomainEvent
    {
        public OrderEvent(Order order) : base(order.Symbol)
        {
            OrderType = order.OrderType;
            Id = order.Id;
            Price = order.Price;
            Size = order.Size;
            TraderId = order.TraderId;
        }

        public OrderType OrderType { get; }
        public long Id { get; }
        public long Price { get; }
        public long Size { get; }
        public int TraderId { get; }

        public Order ToOrder() => new Order(Symbol, Id, OrderType, Price, Size, TraderId);

        protected override IEnumerable<object> GetAllValues()
        {
            yield return CreationDate;
            yield return Symbol;
            yield return OrderType;
            yield return Id;
            yield return Price;
            yield return Size;
            yield return TraderId;
        }
    }
}
