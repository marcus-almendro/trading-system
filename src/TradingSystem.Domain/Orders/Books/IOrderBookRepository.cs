using System.Collections.Generic;

namespace TradingSystem.Domain.Orders.Books
{
    public interface IOrderBookRepository
    {
        void Add(IOrderBook orderBook);
        IOrderBook Get(string symbol);
        IEnumerable<IOrderBook> GetAll();
    }
}
