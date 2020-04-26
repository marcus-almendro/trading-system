using System.Collections.Generic;

namespace TradingSystem.Domain.Common
{
    public abstract class Entity : IEntity
    {
        public IReadOnlyList<DomainEvent> Events { get => InternalEvents.AsReadOnly(); }
        public void ClearEvents() => InternalEvents.Clear();

        protected List<DomainEvent> InternalEvents = new List<DomainEvent>();

        public void CopyAllEventsFrom(IEntity anotherEntity, bool clearAnotherEntityEvents = true)
        {
            InternalEvents.AddRange(anotherEntity.Events);
            if (clearAnotherEntityEvents)
                anotherEntity.ClearEvents();
        }
    }
}
