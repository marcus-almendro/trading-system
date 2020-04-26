using System.Collections.Generic;
using TradingSystem.Application.DTO;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Orders;

namespace TradingSystem.Application.Service
{
    public interface IOrderBookService
    {
        ErrorCodeDTO AddOrderBook(string symbol);
        ErrorCodeDTO AddBuyOrder(BuyOrderCommand buyOrder);
        ErrorCodeDTO AddSellOrder(SellOrderCommand sellOrder);
        ErrorCodeDTO UpdateOrder(UpdateOrderCommand updateOrder);
        ErrorCodeDTO DeleteOrder(DeleteOrderCommand deleteOrder);
        void ApplyEvent(IReadOnlyList<DomainEvent> domainEvents);
        List<string> GetAllOrderBooks();
        int TotalOrders(string symbol);
        List<Order> UnsafeDump(string symbol); //unsafe, only for testing
    }
}