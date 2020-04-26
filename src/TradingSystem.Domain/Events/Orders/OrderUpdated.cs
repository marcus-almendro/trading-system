using TradingSystem.Domain.Orders;

namespace TradingSystem.Domain.Events.Orders
{
    public class OrderUpdated : OrderEvent
    {
        public OrderUpdated(Order order) : base(order) { }
    }
}
