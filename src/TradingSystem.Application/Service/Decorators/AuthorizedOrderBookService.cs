using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using TradingSystem.Application.DTO;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Orders;

namespace TradingSystem.Application.Service.Decorators
{
    public class AuthorizedOrderBookService : IOrderBookService
    {
        private readonly IOrderBookService _inner;
        private readonly ILogger<AuthorizedOrderBookService> _logger;
        private readonly ILifecycleManager _lifecycleManager;

        public AuthorizedOrderBookService(IOrderBookService inner, ILogger<AuthorizedOrderBookService> logger, ILifecycleManager lifecycleManager)
        {
            _inner = inner;
            _logger = logger;
            _lifecycleManager = lifecycleManager;
        }

        public ErrorCodeDTO AddOrderBook(string symbol) => ExecuteAsLeader(() => _inner.AddOrderBook(symbol));
        public ErrorCodeDTO AddBuyOrder(BuyOrderCommand buyOrder) => ExecuteAsLeader(() => _inner.AddBuyOrder(buyOrder));
        public ErrorCodeDTO AddSellOrder(SellOrderCommand sellOrder) => ExecuteAsLeader(() => _inner.AddSellOrder(sellOrder));
        public ErrorCodeDTO UpdateOrder(UpdateOrderCommand updateOrder) => ExecuteAsLeader(() => _inner.UpdateOrder(updateOrder));
        public ErrorCodeDTO DeleteOrder(DeleteOrderCommand deleteOrder) => ExecuteAsLeader(() => _inner.DeleteOrder(deleteOrder));
        public void ApplyEvent(IReadOnlyList<DomainEvent> domainEvents) => ExecuteAsFollower(() => _inner.ApplyEvent(domainEvents));
        public List<string> GetAllOrderBooks() => _inner.GetAllOrderBooks();
        public int TotalOrders(string symbol) => _inner.TotalOrders(symbol);
        public List<Order> UnsafeDump(string symbol) => _inner.UnsafeDump(symbol);

        private ErrorCodeDTO ExecuteAsLeader(Func<ErrorCodeDTO> action)
        {
            if (!_lifecycleManager.IsLeader)
            {
                _logger.LogInformation("Unauthorized Call: ExecuteAsLeader");
                return new ErrorCodeDTO(ErrorCode.OperationDeniedNodeIsNotLeader);
            }
            return action();
        }

        private void ExecuteAsFollower(Action action)
        {
            if (_lifecycleManager.IsLeader)
            {
                _logger.LogInformation("Unauthorized Call: ExecuteAsFollower");
                throw new InvalidOperationException("Unauthorized call: ExecuteAsFollower");
            }
            action();
        }
    }
}
