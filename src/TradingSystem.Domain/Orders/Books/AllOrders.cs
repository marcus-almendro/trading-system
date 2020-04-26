using System.Collections.Generic;

namespace TradingSystem.Domain.Orders.Books
{
    public class AllOrders : Dictionary<long, Order>
    {
        private long _currentId = 0;

        public long CurrentId
        {
            get
            {
                _currentId++;
                return _currentId;
            }
            set { _currentId = value; }
        }
    }
}
