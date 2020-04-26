using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TradingSystem.WebApp.State
{
    public class Offsets
    {
        private readonly ConcurrentDictionary<string, long> _offsets = new ConcurrentDictionary<string, long>();

        public long Get(string symbol) => _offsets.GetValueOrDefault(symbol);
        public void Set(string symbol, long offset) => _offsets[symbol] = offset;
    }
}
