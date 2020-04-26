using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using TradingSystem.Application.DTO;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Orders;

namespace TradingSystem.Application.Service.Decorators
{
    public class SynchronizedOrderBookService : IOrderBookService
    {
        private readonly IOrderBookService _inner;
        private readonly ILogger<SynchronizedOrderBookService> _logger;
        private readonly OrderBookServiceSettings _settings;
        private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

        public SynchronizedOrderBookService(IOrderBookService inner, ILogger<SynchronizedOrderBookService> logger, OrderBookServiceSettings settings)
        {
            _inner = inner;
            _logger = logger;
            _settings = settings;
        }

        public ErrorCodeDTO AddOrderBook(string symbol) => Synchronized(symbol, () => _inner.AddOrderBook(symbol));
        public ErrorCodeDTO AddBuyOrder(BuyOrderCommand buyOrder) => Synchronized(buyOrder.Symbol, () => _inner.AddBuyOrder(buyOrder));
        public ErrorCodeDTO AddSellOrder(SellOrderCommand sellOrder) => Synchronized(sellOrder.Symbol, () => _inner.AddSellOrder(sellOrder));
        public ErrorCodeDTO UpdateOrder(UpdateOrderCommand updateOrder) => Synchronized(updateOrder.Symbol, () => _inner.UpdateOrder(updateOrder));
        public ErrorCodeDTO DeleteOrder(DeleteOrderCommand deleteOrder) => Synchronized(deleteOrder.Symbol, () => _inner.DeleteOrder(deleteOrder));
        public void ApplyEvent(IReadOnlyList<DomainEvent> domainEvents) => Synchronized(domainEvents[0].Symbol, () => _inner.ApplyEvent(domainEvents));
        public List<string> GetAllOrderBooks() => _inner.GetAllOrderBooks();
        public int TotalOrders(string symbol) => _inner.TotalOrders(symbol);
        public List<Order> UnsafeDump(string symbol) => _inner.UnsafeDump(symbol);

        private ErrorCodeDTO Synchronized(string symbol, Func<ErrorCodeDTO> action)
        {
            var lockObj = _locks.GetOrAdd(symbol, s => new object());

            var lockTaken = false;
            try
            {
                Monitor.TryEnter(lockObj, _settings.MillisecondsTimeout, ref lockTaken);
                if (lockTaken)
                {
                    return action();
                }
                else
                {
                    _logger.LogInformation("Cannot get lock!!");
                    return new ErrorCodeDTO(ErrorCode.Timeout);
                }
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(lockObj);
            }
        }

        private void Synchronized(string symbol, Action action)
        {
            var errorCode = Synchronized(symbol, () =>
            {
                action();
                return default;
            });

            if (errorCode != default)
            {
                throw new TimeoutException();
            }
        }
    }
}
