using System.Collections.Generic;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;

namespace TradingSystem.Domain.Orders.Books
{
    public interface IOrderBook : IEntity
    {
        string Symbol { get; }
        int Count { get; }
        ErrorCode Add(OrderType orderType, long price, long size, int traderId);
        ErrorCode Update(long orderId, long size, int traderId);
        ErrorCode Delete(long orderId, int traderId);
        ErrorCode ApplyEvent(OrderCreated orderEvent);
        ErrorCode ApplyEvent(OrderUpdated orderEvent);
        ErrorCode ApplyEvent(OrderDeleted orderEvent);
        List<Order> Dump();
    }
}