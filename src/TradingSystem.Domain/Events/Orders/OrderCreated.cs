using TradingSystem.Domain.Orders;

namespace TradingSystem.Domain.Events.Orders
{
    public class OrderCreated : OrderEvent
    {
        public OrderCreated(Order order) : base(order) { }
    }
}
