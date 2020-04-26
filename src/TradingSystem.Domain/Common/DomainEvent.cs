using System;

namespace TradingSystem.Domain.Common
{
    public abstract class DomainEvent : ValueObject
    {
        protected DomainEvent(string symbol)
        {
            Symbol = symbol;
            CreationDate = Clock.UtcNow;
        }

        public string Symbol { get; }
        public DateTimeOffset CreationDate { get; }
    }
}
