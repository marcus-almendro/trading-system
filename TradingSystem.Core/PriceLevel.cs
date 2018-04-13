using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystem.Core
{
    class PriceLevel
    {
        public Side Side { get; }
        public long Price { get; }
        SortedList<long, Order> _orders = new SortedList<long, Order>();
        Func<long> _getNextTradeId;
        Action<Trade> _onTrade;
        Action<long> _onUpdate, _onDelete;

        public PriceLevel(Func<long> getNextTradeId, Action<Trade> onTrade, Action<long> onUpdate, Action<long> onDelete, Side side, long price)
        {
            _getNextTradeId = getNextTradeId;
            _onTrade = onTrade;
            _onUpdate = onUpdate;
            _onDelete = onDelete;
            Side = side;
            Price = price;
        }

        public int Count => _orders.Count;

        public void Add(Order order)
        {
            _orders.Add(order.Id, order);
        }

        public long Match(long orderId, int userId, Side side, long quantity)
        {
            while (quantity > 0 && _orders.Count > 0)
            {
                Order order = _orders.ElementAt(0).Value;

                if (order.Size > quantity)
                {
                    order.Size -= quantity;
                    _onTrade(new Trade {
                        Id = _getNextTradeId(),
                        TakenOrderId = order.Id,
                        TakerOrderId = orderId,
                        TakerSide = side,
                        Price = Price,
                        ExecutedSize = quantity,
                        RemainingSize = order.Size,
                        TakenUserId = order.UserId,
                        TakerUserId = userId
                    });
                    quantity = 0;
                    _onUpdate(order.Id);
                }
                else
                {
                    _orders.RemoveAt(0);
                    _onTrade(new Trade
                    {
                        Id = _getNextTradeId(),
                        TakenOrderId = order.Id,
                        TakerOrderId = orderId,
                        TakerSide = side,
                        Price = Price,
                        ExecutedSize = order.Size,
                        RemainingSize = 0,
                        TakenUserId = order.UserId,
                        TakerUserId = userId
                    });
                    quantity -= order.Size;
                    _onDelete(order.Id);
                }
            }

            return quantity;
        }

        public void Delete(Order order)
        {
            _orders.Remove(order.Id);
        }

        public int IndexOfKey(long orderId)
        {
            return _orders.IndexOfKey(orderId);
        }
    }
}