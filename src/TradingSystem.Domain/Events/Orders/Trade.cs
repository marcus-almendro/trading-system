using System.Collections.Generic;
using TradingSystem.Domain.Common;

namespace TradingSystem.Domain.Events.Orders
{
    public class Trade : DomainEvent
    {
        public Trade(string symbol, long takerOrderId, long takenOrderId, long price, long executedSize) : base(symbol)
        {
            TakerOrderId = takerOrderId;
            TakenOrderId = takenOrderId;
            Price = price;
            ExecutedSize = executedSize;
        }

        public long TakerOrderId { get; }
        public long TakenOrderId { get; }
        public long Price { get; }
        public long ExecutedSize { get; }

        protected override IEnumerable<object> GetAllValues()
        {
            yield return CreationDate;
            yield return Symbol;
            yield return TakerOrderId;
            yield return TakenOrderId;
            yield return Price;
            yield return ExecutedSize;
        }
    }
}
