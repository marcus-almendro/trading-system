using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TradingSystem.Application.DTO;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Application.Integration.Ports;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;
using TradingSystem.Domain.Orders;
using TradingSystem.Domain.Orders.Books;

namespace TradingSystem.Application.Service
{
    public class OrderBookService : IOrderBookService
    {
        private readonly IOrderBookRepository _orderBookRepository;
        private readonly IEventDispatcher<IReadOnlyList<DomainEvent>> _eventDispatcher;
        private readonly IEventReceiver<IReadOnlyList<DomainEvent>> _eventReceiver;
        private readonly ILogger<OrderBookService> _logger;
        private readonly ILifecycleManager _lifecycleManager;

        public OrderBookService(IOrderBookRepository orderBookRepository, IEventDispatcher<IReadOnlyList<DomainEvent>> eventDispatcher, IEventReceiver<IReadOnlyList<DomainEvent>> eventReceiver, ILifecycleManager lifecycleManager, ILogger<OrderBookService> logger)
        {
            _orderBookRepository = orderBookRepository;
            _eventDispatcher = eventDispatcher;
            _eventReceiver = eventReceiver;
            _lifecycleManager = lifecycleManager;
            _logger = logger;
            _eventReceiver.OnEvent += ApplyEvent;
        }

        public ErrorCodeDTO AddOrderBook(string symbol)
        {
            var orderBook = _orderBookRepository.Get(symbol);

            if (orderBook != null)
                return new ErrorCodeDTO(ErrorCode.AlreadyExists);

            orderBook = new OrderBook(symbol);
            _orderBookRepository.Add(orderBook);

            PublishEvents(orderBook);

            return new ErrorCodeDTO(ErrorCode.Success);
        }

        public ErrorCodeDTO AddBuyOrder(BuyOrderCommand buyOrder) =>
            ExecuteCommand(buyOrder.Symbol, orderBook => orderBook.Add(OrderType.Buy, buyOrder.Price, buyOrder.Size, buyOrder.TraderId));

        public ErrorCodeDTO AddSellOrder(SellOrderCommand sellOrder) =>
            ExecuteCommand(sellOrder.Symbol, orderBook => orderBook.Add(OrderType.Sell, sellOrder.Price, sellOrder.Size, sellOrder.TraderId));

        public ErrorCodeDTO UpdateOrder(UpdateOrderCommand updateOrder) =>
            ExecuteCommand(updateOrder.Symbol, orderBook => orderBook.Update(updateOrder.Id, updateOrder.Size, updateOrder.TraderId));

        public ErrorCodeDTO DeleteOrder(DeleteOrderCommand deleteOrder) =>
            ExecuteCommand(deleteOrder.Symbol, orderBook => orderBook.Delete(deleteOrder.Id, deleteOrder.TraderId));

        public void ApplyEvent(IReadOnlyList<DomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                switch (domainEvent)
                {
                    case OrderBookCreated evt:
                        AddOrderBook(evt.Symbol);
                        break;

                    case OrderEvent evt:
                        ExecuteCommand(evt.Symbol, orderBook => orderBook.ApplyEvent((dynamic)evt));
                        break;
                }
            }
        }

        public List<string> GetAllOrderBooks() => _orderBookRepository.GetAll().Select(o => o.Symbol).ToList();

        public int TotalOrders(string symbol) => _orderBookRepository.Get(symbol)?.Count ?? 0;

        public List<Order> UnsafeDump(string symbol) => _orderBookRepository.Get(symbol)?.Dump() ?? new List<Order>();

        private ErrorCodeDTO ExecuteCommand(string symbol, Func<IOrderBook, ErrorCode> action)
        {
            var orderBook = _orderBookRepository.Get(symbol);

            _logger.LogTrace("Execute, isLeader={isLeader}, orderBook is null? {orderBookNull}", _lifecycleManager.IsLeader, orderBook == null);

            var errorCode = action(orderBook);

            if (errorCode == ErrorCode.Success)
                PublishEvents(orderBook);

            return new ErrorCodeDTO(errorCode);
        }

        private void PublishEvents(IOrderBook orderBook)
        {
            _logger.LogTrace("PublishEvents, isLeader={isLeader}, eventCount = {eventCount}", _lifecycleManager.IsLeader, orderBook.Events.Count);
            if (_lifecycleManager.IsLeader)
            {
                try
                {
                    _eventDispatcher.Dispatch(orderBook.Events);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Cannot replicate events, shutting down application!!!");
                    _lifecycleManager.Shutdown();
                }
            }
            orderBook.ClearEvents();
        }
    }
}
