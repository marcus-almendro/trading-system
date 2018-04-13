using System;
using System.Collections.Generic;
using System.Linq;

namespace TradingSystem.Core
{
    class OrderBook : IOrderBook
    {
        public event EventHandler<Message> OnMessage;
        SortedList<long, PriceLevel> _bids = new SortedList<long, PriceLevel>(Comparer<long>.Create((a, b) => b.CompareTo(a)));
        SortedList<long, PriceLevel> _asks = new SortedList<long, PriceLevel>();
        Dictionary<long, Order> _orders = new Dictionary<long, Order>();
        public long TradeId { get; set; }

        public ErrorCode Enter(Order entryOrder)
        {
            if (entryOrder.Action == OrderEntryType.Undefined)
                return ErrorCode.InvalidArgument;

            if (entryOrder.Action == OrderEntryType.Update)
                return Update(entryOrder.Id, entryOrder.Size);

            if (entryOrder.Action == OrderEntryType.Delete)
                return Update(entryOrder.Id, 0);

            if (_orders.ContainsKey(entryOrder.Id))
                return ErrorCode.AlreadyExists;

            var buyParams = (_asks, new Func<long, bool>(p => p <= entryOrder.Price), _bids);
            var sellParams = (_bids, new Func<long, bool>(p => p >= entryOrder.Price), _asks);

            var (oppositeLevels, priceSelector, sideLevels) = entryOrder.Side == Side.Buy ? buyParams : sellParams;

            var currentSize = entryOrder.Size;

            var bestLevel = oppositeLevels.FirstOrDefault().Value;

            while (currentSize > 0 && bestLevel != null && priceSelector(bestLevel.Price))
            {
                currentSize = bestLevel.Match(entryOrder.Id, entryOrder.UserId, entryOrder.Side, currentSize);

                if (bestLevel.Count == 0)
                    oppositeLevels.Remove(bestLevel.Price);

                bestLevel = oppositeLevels.FirstOrDefault().Value;
            }

            if (currentSize > 0)
            {
                var order = entryOrder.Clone();
                order.Size = currentSize;
                Add(sideLevels, order);
                _orders[order.Id] = order;
                OnMessage?.Invoke(this, new Message(order.Clone()));
            }

            return ErrorCode.Success;
        }

        public List<Order> Dump()
        {
            return _orders.Values.ToList();
        }

        void OnMatch(Trade trade)
        {
            OnMessage?.Invoke(this, new Message(trade));
        }

        void OnUpdateDueMatching(long orderId)
        {
            OnMessage?.Invoke(this, new Message(new Order(_orders[orderId], OrderEntryType.Update)));
        }

        void OnDeleteDueMatching(long orderId)
        {
            OnMessage?.Invoke(this, new Message(new Order(_orders[orderId], OrderEntryType.Delete)));
            _orders.Remove(orderId);
        }

        int GetPosition(Order order)
        {
            var list = order.Side == Side.Buy ? _bids : _asks;
            var index = list.IndexOfKey(order.Price);
            int position = 0;
            for (int i = 0; i <= index; i++)
            {
                if (i == index)
                {
                    position += list.ElementAt(i).Value.IndexOfKey(order.Id);
                }
                else
                {
                    position += list.ElementAt(i).Value.Count;    
                }
            }
            return position;
        }

        ErrorCode Update(long orderId, long size)
        {
            Order order;
            if (!_orders.TryGetValue(orderId, out order))
                return ErrorCode.InvalidOrderId;

            if (size >= order.Size)
            {
                return ErrorCode.InvalidArgument;
            }

            if (size > 0)
            {
                order.Position = GetPosition(order);
                order.Size = size;
                OnMessage?.Invoke(this, new Message(new Order(order, OrderEntryType.Update)));
            }
            else
            {
                Delete(order);
                OnMessage?.Invoke(this, new Message(new Order(order, OrderEntryType.Delete)));
            }
            return ErrorCode.Success;
        }

        void Add(SortedList<long, PriceLevel> levels, Order order)
        {
            PriceLevel level;
            if (!levels.TryGetValue(order.Price, out level))
            {
                level = new PriceLevel(NextTradeId, OnMatch, OnUpdateDueMatching, OnDeleteDueMatching, order.Side, order.Price);
                levels.Add(order.Price, level);
            }
            level.Add(order);
            order.Position = GetPosition(order);
        }

        void Delete(Order order)
        {
            var list = order.Side == Side.Buy ? _bids : _asks;
            order.Position = GetPosition(order);
            var level = list[order.Price];
            level.Delete(order);
            if (level.Count == 0)
                list.Remove(order.Price);
            _orders.Remove(order.Id);
        }

        long NextTradeId()
        {
            return ++TradeId;
        }

        public int Count => _orders.Count;
    }
}