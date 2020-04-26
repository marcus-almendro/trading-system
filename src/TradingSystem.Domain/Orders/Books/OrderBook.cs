using System.Collections.Generic;
using System.Linq;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;
using TradingSystem.Domain.Orders.Books.Sides;

namespace TradingSystem.Domain.Orders.Books
{
    public class OrderBook : Entity, IOrderBook
    {
        private readonly BookSide _buySide, _sellSide;
        private readonly AllOrders _allOrders;

        public OrderBook(string symbol)
        {
            Symbol = symbol;
            _allOrders = new AllOrders();
            _buySide = new BuySide(_allOrders);
            _sellSide = new SellSide(_allOrders);
            InternalEvents.Add(new OrderBookCreated(Symbol));
        }

        public string Symbol { get; }
        public int Count => _allOrders.Keys.Count;

        public ErrorCode Add(OrderType orderType, long price, long size, int traderId) =>
            Add(new Order(Symbol, _allOrders.CurrentId, orderType, price, size, traderId));

        public ErrorCode Update(long orderId, long size, int traderId)
        {
            Logger.ForType<OrderBook>().Debug("Trying to update order {orderId}, size {size}", orderId, size);

            if (!_allOrders.TryGetValue(orderId, out var order))
                return ErrorCode.InvalidOrderId;

            Logger.ForType<OrderBook>().Debug("Updating order {orderId} from size {originalSize} to {size}, trader: {trader}", orderId, order.Size, size, order.TraderId);

            var errorCode = order.Update(size, traderId);

            CopyAllEventsFrom(order);

            return errorCode;
        }

        public ErrorCode Delete(long orderId, int traderId)
        {
            Logger.ForType<OrderBook>().Debug("Trying to delete order {orderId}", orderId);

            if (!_allOrders.TryGetValue(orderId, out var order))
                return ErrorCode.InvalidOrderId;

            Logger.ForType<OrderBook>().Debug("Deleting order {orderId}", orderId);

            ErrorCode errorCode = order.Delete(traderId);
            if (errorCode != ErrorCode.Success)
                return errorCode;

            CopyAllEventsFrom(order);

            var sameBookSide = SameBookSide(order);
            sameBookSide.Delete(order);

            return ErrorCode.Success;
        }

        public ErrorCode ApplyEvent(OrderCreated evt)
        {
            Logger.ForType<OrderBook>().Debug("Applying event {evt}", evt);

            _allOrders.CurrentId = evt.Id;
            return Add(evt.ToOrder());
        }

        public ErrorCode ApplyEvent(OrderUpdated evt)
        {
            Logger.ForType<OrderBook>().Debug("Applying event {evt}", evt);

            return Update(evt.Id, evt.Size, evt.TraderId);
        }

        public ErrorCode ApplyEvent(OrderDeleted evt)
        {
            Logger.ForType<OrderBook>().Debug("Applying event {evt}", evt);

            return Delete(evt.Id, evt.TraderId);
        }

        public List<Order> Dump()
        {
            Logger.ForType<OrderBook>().Debug("Dumping all orders!");
            return _allOrders.Values.ToList();
        }

        private ErrorCode Add(Order order)
        {
            Logger.ForType<OrderBook>().Debug("Adding order {order}", order);

            if (_allOrders.ContainsKey(order.Id))
                return ErrorCode.AlreadyExists;

            Logger.ForType<OrderBook>().Debug("Matching order: {order}", order);
            var otherBookSide = OtherBookSide(order);
            otherBookSide.Match(order);
            CopyAllEventsFrom(otherBookSide);
            order.ClearEvents();

            Logger.ForType<OrderBook>().Debug("Adding remaining order: {order}", order);
            if (order.Size > 0)
            {
                var sameBookSide = SameBookSide(order);
                sameBookSide.Add(order);
                _allOrders[order.Id] = order;
                InternalEvents.Add(new OrderCreated(order));
            }

            return ErrorCode.Success;
        }

        private BookSide SameBookSide(Order order) => order.OrderType == OrderType.Buy ? _buySide : _sellSide;
        private BookSide OtherBookSide(Order order) => order.OrderType == OrderType.Sell ? _buySide : _sellSide;
    }
}