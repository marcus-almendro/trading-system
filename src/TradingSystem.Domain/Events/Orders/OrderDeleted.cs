using TradingSystem.Domain.Orders;

namespace TradingSystem.Domain.Events.Orders
{
    public class OrderDeleted : OrderEvent
    {
        public OrderDeleted(Order order) : base(order) { }
    }
}
