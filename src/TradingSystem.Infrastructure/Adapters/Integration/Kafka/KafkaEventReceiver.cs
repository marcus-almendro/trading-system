using AutoMapper;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TradingSystem.Application.Integration;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Infrastructure.Adapters.Integration.Kafka
{
    public class KafkaEventReceiver<TDestination> : EventReceiver<DomainEventCollection, TDestination>
    {
        private readonly string _brokerList;
        private readonly string _topic;
        private readonly IMapper _mapper;
        private readonly ILogger<KafkaEventReceiver<TDestination>> _logger;
        private IConsumer<string, byte[]> _consumer;

        public KafkaEventReceiver(KafkaAdapterSettings settings, IMapper mapper, ILifecycleManager lifecycleManager, ILogger<KafkaEventReceiver<TDestination>> logger)
            : base(lifecycleManager, logger)
        {
            _mapper = mapper;
            _topic = settings.EventsTopic;
            _brokerList = settings.BrokerList;
            _logger = logger;
        }

        protected override void BeginFollowing()
        {
            _logger.LogInformation("Begin following, building kafka consumer");
            _consumer = new ConsumerBuilder<string, byte[]>(new ConsumerConfig
            {
                GroupId = Guid.NewGuid().ToString(),
                BootstrapServers = _brokerList,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
            }).Build();

            _consumer.Assign(new TopicPartition(_topic, new Partition(0)));
            _logger.LogInformation("Partition 0 assigned");
        }

        protected override DomainEventCollection ConsumeNextMessage()
        {
            var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(200));
            var message = consumeResult?.Value;
            var col = message != null ? DomainEventCollection.Parser.ParseFrom(message) : null;
            if (col != null)
                col.Offset = consumeResult?.Offset.Value ?? 0;
            return col;
        }

        protected override void WaitConsumptionEnd(long maxOffset)
        {
            var topicPartition = _consumer.Assignment.Single();
            var waterMarks = _consumer.QueryWatermarkOffsets(topicPartition, TimeSpan.FromSeconds(30));

            _logger.LogDebug("Wait consumption end params waterMarks: {waterMarks}, consumerPosition: {consumerPosition}", waterMarks, _consumer.Position(topicPartition));
            if (_consumer.Position(topicPartition) == Offset.Unset && waterMarks.High == new Offset(0))
                return;

            while (_consumer.Position(topicPartition) != waterMarks.High && IsRunning)
            {
                if (_consumer.Position(topicPartition).Value > maxOffset && maxOffset != -1)
                    break;

                Task.Delay(100).Wait();
            }
        }

        protected override void BeginStopping() => _consumer.Unsubscribe();

        protected override TDestination Map(DomainEventCollection obj) => _mapper.Map<TDestination>(obj);
    }
}
