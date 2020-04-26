using System.Collections.Generic;
using TradingSystem.Domain.Common;

namespace TradingSystem.Domain.Orders.Books.Sides
{
    internal class BookSide : Entity
    {
        private readonly AllOrders _allOrders;
        private readonly SortedList<long, PriceLevel> _priceLevels;

        protected BookSide(IComparer<long> comparer, AllOrders allOrders)
        {
            _priceLevels = new SortedList<long, PriceLevel>(comparer);
            _allOrders = allOrders;
        }

        public void Add(Order order)
        {
            if (!_priceLevels.TryGetValue(order.Price, out var level))
            {
                Logger.ForType<BookSide>().Debug("Creating new price level for {symbol} at price {price}", order.Symbol, order.Price);
                level = new PriceLevel(order.Price, _allOrders);
                _priceLevels.Add(order.Price, level);
            }
            level.Add(order);
        }

        public void Match(Order order)
        {
            var emptyLevels = new List<long>();

            Logger.ForType<BookSide>().Debug("Matching order: {order}", order);

            foreach (var bestLevel in _priceLevels.Values)
            {
                Logger.ForType<BookSide>().Debug("Best level price: {price}, current order size: {size}", bestLevel.Price, order.Size);

                if (order.Size == 0 || _priceLevels.Comparer.Compare(bestLevel.Price, order.Price) > 0)
                    break;

                bestLevel.Match(order);
                CopyAllEventsFrom(bestLevel);

                Logger.ForType<BookSide>().Debug("Best level empty: {empty}", bestLevel.Empty);

                if (bestLevel.Empty)
                    emptyLevels.Add(bestLevel.Price);
            }

            emptyLevels.ForEach(level => _priceLevels.Remove(level));

            Logger.ForType<BookSide>().Debug("End of order matching: {order}", order);
        }

        public void Delete(Order order)
        {
            Logger.ForType<BookSide>().Debug("Deleting order: {order}", order);
            var priceLevel = _priceLevels[order.Price];
            priceLevel.Delete(order);
            if (priceLevel.Empty)
                _priceLevels.Remove(priceLevel.Price);
        }
    }
}
