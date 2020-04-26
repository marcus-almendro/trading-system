using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TradingSystem.Application.DTO;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Orders;

namespace TradingSystem.Application.Service.Decorators
{
    public class LoggedOrderBookService : IOrderBookService
    {
        private readonly IOrderBookService _inner;
        private readonly ILogger<LoggedOrderBookService> _logger;

        public LoggedOrderBookService(IOrderBookService inner, ILogger<LoggedOrderBookService> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public ErrorCodeDTO AddOrderBook(string symbol) => LogCall(symbol, () => _inner.AddOrderBook(symbol));
        public ErrorCodeDTO AddBuyOrder(BuyOrderCommand buyOrder) => LogCall(buyOrder, () => _inner.AddBuyOrder(buyOrder));
        public ErrorCodeDTO AddSellOrder(SellOrderCommand sellOrder) => LogCall(sellOrder, () => _inner.AddSellOrder(sellOrder));
        public ErrorCodeDTO UpdateOrder(UpdateOrderCommand updateOrder) => LogCall(updateOrder, () => _inner.UpdateOrder(updateOrder));
        public ErrorCodeDTO DeleteOrder(DeleteOrderCommand deleteOrder) => LogCall(deleteOrder, () => _inner.DeleteOrder(deleteOrder));
        public void ApplyEvent(IReadOnlyList<DomainEvent> domainEvents) => LogCall(domainEvents, () => _inner.ApplyEvent(domainEvents));
        public List<string> GetAllOrderBooks() => LogCall(() => _inner.GetAllOrderBooks());
        public int TotalOrders(string symbol) => LogCall(symbol, () => _inner.TotalOrders(symbol));
        public List<Order> UnsafeDump(string symbol) => LogCall(symbol, () => _inner.UnsafeDump(symbol));

        private T LogCall<T>(object obj, Func<T> action, [CallerMemberName] string memberName = "")
        {
            using (_logger.BeginScope($"SCOPE: {memberName} {Guid.NewGuid()}"))
            {
                _logger.LogTrace("request: {obj}", obj);
                var resp = action();
                _logger.LogTrace("response: {resp}", resp);
                return resp;
            }
        }

        private T LogCall<T>(Func<T> action, [CallerMemberName] string memberName = "")
        {
            using (_logger.BeginScope($"SCOPE: {memberName} {Guid.NewGuid()}"))
            {
                var resp = action();
                _logger.LogTrace("response: {resp}", resp);
                return resp;
            }
        }

        private void LogCall(object obj, Action action, [CallerMemberName] string memberName = "")
        {
            using (_logger.BeginScope($"SCOPE: {memberName} {Guid.NewGuid()}"))
            {
                _logger.LogTrace("request: {obj}", obj);
                action();
            }
        }
    }
}
