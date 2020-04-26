using System.Collections.Generic;
using TradingSystem.Domain.Common;

namespace TradingSystem.Domain.Events.Orders
{
    public class OrderBookCreated : DomainEvent
    {
        public OrderBookCreated(string symbol) : base(symbol) { }

        protected override IEnumerable<object> GetAllValues()
        {
            yield return CreationDate;
            yield return Symbol;
        }
    }
}
