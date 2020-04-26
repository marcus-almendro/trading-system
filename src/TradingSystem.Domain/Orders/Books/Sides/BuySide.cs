using System.Collections.Generic;

namespace TradingSystem.Domain.Orders.Books.Sides
{
    internal class BuySide : BookSide
    {
        public BuySide(AllOrders allOrders) : base(Comparer<long>.Create((a, b) => b.CompareTo(a)), allOrders)
        {
        }
    }
}
