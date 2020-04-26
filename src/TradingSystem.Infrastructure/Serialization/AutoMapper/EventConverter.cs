using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;

namespace TradingSystem.Infrastructure.Serialization.AutoMapper
{
    internal class EventConverter : ITypeConverter<IReadOnlyList<DomainEvent>, DomainEventCollection>, ITypeConverter<DomainEventCollection, IReadOnlyList<DomainEvent>>
    {
        private static long _messageId = 0;
        public DomainEventCollection Convert(IReadOnlyList<DomainEvent> domainEvents, DomainEventCollection destination, ResolutionContext context)
        {
            var collection = new DomainEventCollection();
            collection.MessageId = Interlocked.Increment(ref _messageId);
            collection.Events.AddRange(domainEvents.Select(d => context.Mapper.Map<DomainEventMsg>(d)));
            return collection;
        }

        public IReadOnlyList<DomainEvent> Convert(DomainEventCollection domainEventCollection, IReadOnlyList<DomainEvent> destination, ResolutionContext context)
        {
            Interlocked.Increment(ref _messageId);
            return domainEventCollection.Events.Select(domainEventMsg =>
            (DomainEvent)(domainEventMsg.EventTypeCase switch
            {
                DomainEventMsg.EventTypeOneofCase.None => null,
                DomainEventMsg.EventTypeOneofCase.OrderCreated => context.Mapper.Map<OrderCreated>(domainEventMsg),
                DomainEventMsg.EventTypeOneofCase.OrderUpdated => context.Mapper.Map<OrderUpdated>(domainEventMsg),
                DomainEventMsg.EventTypeOneofCase.OrderDeleted => context.Mapper.Map<OrderDeleted>(domainEventMsg),
                DomainEventMsg.EventTypeOneofCase.Trade => context.Mapper.Map<Trade>(domainEventMsg),
                DomainEventMsg.EventTypeOneofCase.OrderBookCreated => context.Mapper.Map<OrderBookCreated>(domainEventMsg),
                _ => throw new NotImplementedException()
            })).ToList().AsReadOnly();
        }
    }
}
