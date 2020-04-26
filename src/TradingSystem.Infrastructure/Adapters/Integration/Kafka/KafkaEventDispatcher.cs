using AutoMapper;
using Confluent.Kafka;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using TradingSystem.Application.Integration;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Domain.Common;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Infrastructure.Adapters.Integration.Kafka
{
    public class KafkaEventDispatcher : EventDispatcher<IReadOnlyList<DomainEvent>, DomainEventCollection>
    {
        private readonly string _topic;
        private readonly string _brokerList;
        private readonly IMapper _mapper;
        private readonly ILogger<KafkaEventDispatcher> _logger;
        private IProducer<string, byte[]> _producer;

        public KafkaEventDispatcher(KafkaAdapterSettings settings, IMapper mapper, ILifecycleManager lifecycleManager, ILogger<KafkaEventDispatcher> logger)
            : base(lifecycleManager, logger)
        {
            _mapper = mapper;
            _topic = settings.EventsTopic;
            _brokerList = settings.BrokerList;
            _logger = logger;
        }

        protected override void BecomingLeader()
        {
            _logger.LogInformation("Becoming leader, creating kafka producer");
            _producer = new ProducerBuilder<string, byte[]>(
                new ProducerConfig
                {
                    BootstrapServers = _brokerList,
                    EnableIdempotence = true,
                }).Build();
        }

        protected override void Publish(DomainEventCollection msg)
        {
            _logger.LogDebug("Publishing message {msg}", msg);
            var dr = _producer.ProduceAsync(_topic, new Message<string, byte[]> { Key = msg.Events[0].Symbol, Value = msg.ToByteArray() }).Result;
            if (dr.Status != PersistenceStatus.Persisted)
                throw new Exception(dr.Status.ToString());
        }

        protected override DomainEventCollection Map(IReadOnlyList<DomainEvent> obj) => _mapper.Map<DomainEventCollection>(obj);
    }
}
