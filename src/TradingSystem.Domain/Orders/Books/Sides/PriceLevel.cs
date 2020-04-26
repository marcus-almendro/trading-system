using System;
using System.Collections.Generic;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;

namespace TradingSystem.Domain.Orders.Books.Sides
{
    internal class PriceLevel : Entity
    {
        private readonly AllOrders _allOrders;
        private readonly List<long> _priceLevelOrders;

        public PriceLevel(long price, AllOrders orders)
        {
            Price = price;
            _allOrders = orders;
            _priceLevelOrders = new List<long>();
        }

        public long Price { get; }
        public bool Empty => _priceLevelOrders.Count == 0;

        public void Add(Order order)
        {
            var x = _priceLevelOrders.BinarySearch(order.Id);
            _priceLevelOrders.Insert(x >= 0 ? x : ~x, order.Id);
        }

        public void Match(Order takerOrder)
        {
            while (takerOrder.Size > 0 && _priceLevelOrders.Count > 0)
            {
                var takenOrder = _allOrders[_priceLevelOrders[0]];

                Logger.ForType<PriceLevel>().Debug("TakerOrder: {taker}, TakenOrder: {taken}, PriceLevelCount: {plc}", takerOrder, takenOrder, _priceLevelOrders.Count);

                var executedSize = Math.Min(takerOrder.Size, takenOrder.Size);
                takenOrder.Update(takenOrder.Size - executedSize);
                takerOrder.Update(takerOrder.Size - executedSize);

                CopyAllEventsFrom(takenOrder);
                InternalEvents.Add(new Trade(takenOrder.Symbol, takerOrder.Id, takenOrder.Id, Price, executedSize));

                if (takenOrder.IsDeleted)
                    Delete(takenOrder);
            }
            Logger.ForType<PriceLevel>().Debug("TakerOrder: {taker}, PriceLevelCount: {plc}", takerOrder, _priceLevelOrders.Count);
        }

        public void Delete(Order order)
        {
            _priceLevelOrders.Remove(order.Id);
            _allOrders.Remove(order.Id);
        }
    }
}