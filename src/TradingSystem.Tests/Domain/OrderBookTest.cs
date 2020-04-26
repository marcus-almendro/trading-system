using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;
using TradingSystem.Domain.Orders;
using TradingSystem.Domain.Orders.Books;
using Xunit;

namespace TradingSystem.Tests.Domain
{
    public class OrderBookTest
    {
        private readonly string _symbol = "test";
        private OrderBook _orderBook;
        private readonly List<DomainEvent> _lastEvents;

        public OrderBookTest()
        {
            _lastEvents = new List<DomainEvent>();
            Clock.UseTestClock = true;
            _orderBook = new OrderBook(_symbol);
            _orderBook.ClearEvents();
        }

        [Fact]
        public void TotalMatch()
        {
            var createdBuyOrder = new OrderCreated(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 100, traderId: 0));
            var deletedSellOrder = new OrderDeleted(new Order(symbol: _symbol, id: 2, orderType: OrderType.Sell, price: 100, size: 0, traderId: 1));
            var deletedBuyOrder = new OrderDeleted(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 0, traderId: 0));
            var expectedTrade = new Trade(symbol: _symbol, takerOrderId: deletedSellOrder.Id, takenOrderId: deletedBuyOrder.Id, price: 100, executedSize: 100);

            _orderBook.Add(orderType: OrderType.Buy, price: 100, size: 100, traderId: 0).Should().Be(ErrorCode.Success);
            _orderBook.Events.OfType<OrderCreated>().Single().Should().Be(createdBuyOrder);
            _orderBook.Count.Should().Be(1);
            PublishEventsAndClear();

            _orderBook.Add(orderType: OrderType.Sell, price: 100, size: 100, traderId: 1).Should().Be(ErrorCode.Success);
            _orderBook.Events.OfType<OrderDeleted>().Single().Should().Be(deletedBuyOrder);
            _orderBook.Events.OfType<Trade>().Single().Should().Be(expectedTrade);
            _orderBook.Count.Should().Be(0);
            PublishEventsAndClear();
        }

        [Fact]
        public void PartialMatch()
        {
            var createdBuyOrder = new OrderCreated(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 100, traderId: 0));
            var updatedBuyOrder = new OrderUpdated(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 50, traderId: 0));
            var deletedSellOrder = new Order(symbol: _symbol, id: 2, orderType: OrderType.Sell, price: 90, size: 0, traderId: 1);
            var expectedTrade = new Trade(symbol: _symbol, takerOrderId: deletedSellOrder.Id, takenOrderId: updatedBuyOrder.Id, price: 100, executedSize: 50);

            _orderBook.Add(orderType: OrderType.Buy, price: 100, size: 100, traderId: 0).Should().Be(ErrorCode.Success);
            _orderBook.Events.OfType<OrderCreated>().Single().Should().Be(createdBuyOrder);
            _orderBook.Count.Should().Be(1);
            PublishEventsAndClear();

            _orderBook.Add(orderType: OrderType.Sell, price: 90, size: 50, traderId: 1).Should().Be(ErrorCode.Success);
            _orderBook.Events.OfType<OrderUpdated>().Single().Should().Be(updatedBuyOrder);
            _orderBook.Events.OfType<Trade>().Single().Should().Be(expectedTrade);
            _orderBook.Count.Should().Be(1);
            PublishEventsAndClear();
        }

        [Fact]
        public void OverMatchAfterUpdating()
        {
            var createdBuyOrder = new OrderCreated(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 100, traderId: 0));
            var updatedBuyOrder = new OrderUpdated(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 50, traderId: 0));
            var createdSellOrder = new OrderCreated(new Order(symbol: _symbol, id: 2, orderType: OrderType.Sell, price: 90, size: 50, traderId: 1));
            var deletedBuyOrder = new OrderDeleted(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 0, traderId: 0));
            var expectedTrade = new Trade(symbol: _symbol, takerOrderId: createdSellOrder.Id, takenOrderId: deletedBuyOrder.Id, price: 100, executedSize: 50);

            _orderBook.Add(orderType: OrderType.Buy, price: 100, size: 100, traderId: 0).Should().Be(ErrorCode.Success);
            _orderBook.Events.OfType<OrderCreated>().Single().Should().Be(createdBuyOrder);
            _orderBook.Count.Should().Be(1);
            PublishEventsAndClear();

            _orderBook.Update(1, 50, 0).Should().Be(ErrorCode.Success);
            _orderBook.Events.OfType<OrderUpdated>().Single().Should().Be(updatedBuyOrder);
            _orderBook.Count.Should().Be(1);
            PublishEventsAndClear();

            _orderBook.Add(orderType: OrderType.Sell, price: 90, size: 100, traderId: 1).Should().Be(ErrorCode.Success);
            _orderBook.Events.OfType<OrderDeleted>().Single().Should().Be(deletedBuyOrder);
            _orderBook.Events.OfType<OrderCreated>().Single().Should().Be(createdSellOrder);
            _orderBook.Events.OfType<Trade>().Single().Should().Be(expectedTrade);
            _orderBook.Count.Should().Be(1);
            PublishEventsAndClear();
        }

        [Fact]
        public void OrderAlreadyExists()
        {
            _orderBook.Add(orderType: OrderType.Buy, price: 100, size: 100, traderId: 0).Should().Be(ErrorCode.Success);
            _orderBook.ApplyEvent(new OrderCreated(new Order(_symbol, 1, OrderType.Buy, 100, 100, 0))).Should().Be(ErrorCode.AlreadyExists);
        }

        [Fact]
        public void AddOrder()
        {
            var createdBuyOrder = new OrderCreated(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 100, traderId: 0));

            _orderBook.Add(orderType: OrderType.Buy, price: 100, size: 100, traderId: 0).Should().Be(ErrorCode.Success);
            _orderBook.Events.OfType<OrderCreated>().Single().Should().Be(createdBuyOrder);
            _orderBook.Count.Should().Be(1);
            PublishEventsAndClear();

            _orderBook.Symbol.Should().Be(_symbol);
            _orderBook.Delete(11, 0).Should().Be(ErrorCode.InvalidOrderId);
        }

        [Fact]
        public void DeleteOrder()
        {
            var deletedBuyOrder = new OrderDeleted(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 0, traderId: 0));

            _orderBook.Add(orderType: OrderType.Buy, price: 100, size: 100, traderId: 0).Should().Be(ErrorCode.Success);
            _orderBook.Count.Should().Be(1);
            _orderBook.Delete(1, 1).Should().Be(ErrorCode.Unauthorized);
            _orderBook.Delete(1, 0).Should().Be(ErrorCode.Success);
            _orderBook.Count.Should().Be(0);
            _orderBook.Events.OfType<OrderDeleted>().Single().Should().Be(deletedBuyOrder);
            PublishEventsAndClear();

            _orderBook.Delete(1, 0).Should().Be(ErrorCode.InvalidOrderId);

        }

        [Fact]
        public void UpdateOrder()
        {
            var createdBuyOrder = new OrderCreated(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 100, traderId: 0));
            var updatedBuyOrder = new OrderUpdated(new Order(symbol: _symbol, id: 1, orderType: OrderType.Buy, price: 100, size: 50, traderId: 0));

            _orderBook.Add(orderType: OrderType.Buy, price: 100, size: 100, traderId: 0).Should().Be(ErrorCode.Success);
            _orderBook.Count.Should().Be(1);
            _orderBook.Events.OfType<OrderCreated>().Single().Should().Be(createdBuyOrder);
            _orderBook.Update(1, 50, 0).Should().Be(ErrorCode.Success);
            _orderBook.Count.Should().Be(1);
            _orderBook.Events.OfType<OrderUpdated>().Single().Should().Be(updatedBuyOrder);
            PublishEventsAndClear();

            _orderBook.Update(1, 40, 1).Should().Be(ErrorCode.Unauthorized);
            _orderBook.Update(1, 5000, 0).Should().Be(ErrorCode.InvalidArgument);
            _orderBook.Update(1, -1, 0).Should().Be(ErrorCode.InvalidArgument);
            _orderBook.Update(1, 0, 0).Should().Be(ErrorCode.Success);
            _orderBook.Update(2, 1, 0).Should().Be(ErrorCode.InvalidOrderId);
        }

        [Fact]
        public void ApplyEvents()
        {
            var orderBookCopy = new OrderBook("test");

            OverMatchAfterUpdating();

            _lastEvents.OfType<OrderEvent>().ToList().ForEach(evt => orderBookCopy.ApplyEvent((dynamic)evt));

            orderBookCopy.Count.Should().Be(1);
            orderBookCopy.Dump().Should().BeEquivalentTo(_orderBook.Dump());
            orderBookCopy.Events.Count.Should().BeGreaterThan(0);
        }

        private void PublishEventsAndClear()
        {
            _lastEvents.AddRange(_orderBook.Events);
            _orderBook.ClearEvents();
        }

        [Fact]
        public void ShouldRaiseOrderCreatedEvent()
        {
            _orderBook = new OrderBook(_symbol);
            _orderBook.Events.Count.Should().Be(1);
            _orderBook.Events[0].Should().Be(new OrderBookCreated(_symbol));
        }
    }
}
