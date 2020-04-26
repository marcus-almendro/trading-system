using System.Collections.Generic;

namespace TradingSystem.Domain.Orders.Books.Sides
{
    internal class SellSide : BookSide
    {
        public SellSide(AllOrders allOrders) : base(Comparer<long>.Default, allOrders)
        {
        }
    }
}
