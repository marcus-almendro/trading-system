using System.Collections.Generic;

namespace TradingSystem.Domain.Common
{
    public interface IEntity
    {
        IReadOnlyList<DomainEvent> Events { get; }
        void ClearEvents();
        void CopyAllEventsFrom(IEntity anotherEntity, bool clearAnotherEntityEvents = true);
    }
}