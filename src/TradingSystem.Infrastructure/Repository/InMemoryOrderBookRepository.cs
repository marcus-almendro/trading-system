using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TradingSystem.Domain.Orders.Books;

namespace TradingSystem.Infrastructure.Repository
{
    public class InMemoryOrderBookRepository : IOrderBookRepository
    {
        private readonly ConcurrentDictionary<string, IOrderBook> _books = new ConcurrentDictionary<string, IOrderBook>();
        private readonly ILogger<InMemoryOrderBookRepository> _logger;

        public InMemoryOrderBookRepository(ILogger<InMemoryOrderBookRepository> logger)
        {
            _logger = logger;
        }

        public void Add(IOrderBook orderBook)
        {
            _logger.LogInformation("Adding order book for {symbol} to in memory repository", orderBook.Symbol);
            _books[orderBook.Symbol] = orderBook;
        }

        public IOrderBook Get(string symbol)
        {
            _logger.LogTrace("Retrieving {symbol} from in memory repository", symbol);
            if (!_books.ContainsKey(symbol))
                return null;

            return _books[symbol];
        }

        public IEnumerable<IOrderBook> GetAll() => _books.Values.ToList();
    }
}
