using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradingSystem.Application.DTO;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Application.Integration.Ports;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Application.Service;
using TradingSystem.Application.Service.Decorators;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;
using TradingSystem.Domain.Orders;
using TradingSystem.Domain.Orders.Books;
using Xunit;

namespace TradingSystem.Tests.Application
{
    public class OrderBookServiceTest
    {
        private readonly string _symbol = "test";
        private readonly Mock<IEventDispatcher<IReadOnlyList<DomainEvent>>> _eventDispatcherMoq;
        private readonly Mock<IEventReceiver<IReadOnlyList<DomainEvent>>> _eventReceiverMoq;
        private readonly Mock<ILifecycleManager> _lifecycleManager;
        private readonly IOrderBookService _service;
        private readonly Mock<IOrderBook> _orderBookMoq;
        private readonly Mock<IOrderBookRepository> _repoMoq;

        public OrderBookServiceTest()
        {
            _orderBookMoq = new Mock<IOrderBook>();
            _repoMoq = new Mock<IOrderBookRepository>();
            _eventDispatcherMoq = new Mock<IEventDispatcher<IReadOnlyList<DomainEvent>>>();
            _eventReceiverMoq = new Mock<IEventReceiver<IReadOnlyList<DomainEvent>>>();
            _lifecycleManager = new Mock<ILifecycleManager>();
            _service = new SynchronizedOrderBookService(new OrderBookService(
                _repoMoq.Object,
                _eventDispatcherMoq.Object,
                _eventReceiverMoq.Object,
                _lifecycleManager.Object,
                new NullLogger<OrderBookService>()), new NullLogger<SynchronizedOrderBookService>(), new OrderBookServiceSettings() { MillisecondsTimeout = 500 });
            _lifecycleManager.Setup(p => p.IsLeader).Returns(true);
        }

        [Fact]
        public void AddingOrderShouldPublishEvent()
        {
            var buyOrderCommand = new BuyOrderCommand() { Symbol = _symbol, Price = 100, Size = 100, TraderId = 0 };
            var success = new ErrorCodeDTO(ErrorCode.Success);
            var orderCreated = default(OrderCreated);
            var events = new List<DomainEvent> { orderCreated };

            _repoMoq.Setup(p => p.Get(_symbol)).Returns(_orderBookMoq.Object);
            _orderBookMoq.Setup(p => p.Add(OrderType.Buy, 100, 100, 0)).Returns(ErrorCode.Success);
            _orderBookMoq.Setup(p => p.Events).Returns(events);

            _service.AddBuyOrder(buyOrderCommand).Should().BeEquivalentTo(success);
            _eventDispatcherMoq.Verify(p => p.Dispatch(events));
            _orderBookMoq.Verify(p => p.ClearEvents());
        }

        [Fact]
        public void ServiceIsNotReentrantAndShouldTimeout()
        {
            var manualResetEvent = new ManualResetEvent(false);
            var buyOrderCommand = new BuyOrderCommand() { Symbol = _symbol, Price = 100, Size = 100, TraderId = 0 };
            var success = new ErrorCodeDTO(ErrorCode.Success);
            var timeout = new ErrorCodeDTO(ErrorCode.Timeout);
            var orderCreated = default(OrderCreated);
            var events = new List<DomainEvent> { orderCreated };

            _repoMoq.Setup(p => p.Get(_symbol)).Returns(_orderBookMoq.Object);
            _orderBookMoq.Setup(p => p.Add(OrderType.Buy, 100, 100, 0)).Callback(() =>
            {
                manualResetEvent.Set();
                Task.Delay(1000).Wait();
            }).Returns(ErrorCode.Success);
            _orderBookMoq.Setup(p => p.Events).Returns(events);

            Task.WaitAll(
                Task.Run(() => _service.AddBuyOrder(buyOrderCommand).Should().BeEquivalentTo(success)),
                Task.Run(() =>
                {
                    manualResetEvent.WaitOne();
                    _service.AddBuyOrder(buyOrderCommand).Should().BeEquivalentTo(timeout);
                }));
        }

        [Fact]
        public void InCaseEventDispatcherFailsThenApplicationMustShutdown()
        {
            var buyOrderCommand = new BuyOrderCommand() { Symbol = _symbol, Price = 100, Size = 100, TraderId = 0 };
            var orderCreated = default(OrderCreated);
            var events = new List<DomainEvent> { orderCreated };

            _repoMoq.Setup(p => p.Get(_symbol)).Returns(_orderBookMoq.Object);
            _orderBookMoq.Setup(p => p.Add(OrderType.Buy, 100, 100, 0)).Returns(ErrorCode.Success);
            _orderBookMoq.Setup(p => p.Events).Returns(events);
            _eventDispatcherMoq.Setup(p => p.Dispatch(events)).Throws<Exception>();

            _service.AddBuyOrder(buyOrderCommand);

            _lifecycleManager.Verify(p => p.Shutdown());
        }
    }
}
