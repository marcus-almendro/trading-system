using AutoMapper;
using FluentAssertions;
using Google.Protobuf;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;
using TradingSystem.Domain.Orders;
using TradingSystem.Infrastructure.Adapters.Integration.Kafka;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Tests.Utils;
using Xunit;

namespace TradingSystem.Tests.Integration.Kafka
{
    public class KafkaEventDispatcherTest : IDisposable
    {
        private readonly KafkaEventDispatcher _dispatcher;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILifecycleManager> _lifecycleManager;
        private readonly OrderCreated _orderCreated;
        private readonly DomainEventMsg _orderCreatedMsg;
        private readonly OrderUpdated _orderUpdated;
        private readonly DomainEventMsg _orderUpdatedMsg;
        private readonly IReadOnlyList<DomainEvent> _events;
        private readonly DomainEventCollection _domainEventCollection;

        public KafkaEventDispatcherTest()
        {
            DockerUtils.StartDockerContainers();
            DockerUtils.WaitTopicCreation(SettingsParser.KafkaSettings);
            _mapper = new Mock<IMapper>();
            _lifecycleManager = new Mock<ILifecycleManager>();
            _dispatcher = new KafkaEventDispatcher(SettingsParser.KafkaSettings, _mapper.Object, _lifecycleManager.Object, new NullLogger<KafkaEventDispatcher>());
            _orderCreated = new OrderCreated(new Order("test", 0, TradingSystem.Domain.Orders.OrderType.Buy, 10, 10, 1));
            _orderCreatedMsg = new DomainEventMsg
            {
                Symbol = "test",
                CreationDate = DateTimeOffset.MinValue.UtcTicks,
                OrderCreated = new OrderCreatedEventMsg
                {
                    Id = 0,
                    Price = 10,
                    Size = 10,
                    TraderId = 1
                }
            };
            _orderUpdated = new OrderUpdated(new Order("test", 0, TradingSystem.Domain.Orders.OrderType.Buy, 5, 5, 1));
            _orderUpdatedMsg = new DomainEventMsg
            {
                Symbol = "test",
                CreationDate = DateTimeOffset.MinValue.UtcTicks,
                OrderUpdated = new OrderUpdatedEventMsg
                {
                    Id = 0,
                    Price = 5,
                    Size = 5,
                    TraderId = 1
                }
            };
            _events = new List<DomainEvent> { _orderCreated, _orderUpdated }.AsReadOnly();
            _domainEventCollection = new DomainEventCollection();
            _domainEventCollection.Events.Add(_orderCreatedMsg);
            _domainEventCollection.Events.Add(_orderUpdatedMsg);

            _mapper.Setup(p => p.Map<DomainEventCollection>(_events)).Returns(_domainEventCollection);
        }

        public void Dispose()
        {
            DockerUtils.CleanupCurrent();
        }

        [Fact]
        public void ShouldBeAbleToWriteToKafka()
        {
            _lifecycleManager.Raise(p => p.StatusChanged += null, Status.BecomingLeader);
            _dispatcher.Dispatch(_events);

            var messages = DockerUtils.GetMessages<string, byte[]>("localhost:9092", "test_topic", 1);
            messages[0].Key.Should().Be("test");
            messages[0].Value.Should().BeEquivalentTo(_domainEventCollection.ToByteArray());
        }

        [Fact]
        public void ShouldBeAbleToWriteToKafkaMultipleMessages()
        {
            _lifecycleManager.Raise(p => p.StatusChanged += null, Status.BecomingLeader);
            _dispatcher.Dispatch(_events);
            _dispatcher.Dispatch(_events);

            var messages = DockerUtils.GetMessages<string, byte[]>("localhost:9092", "test_topic", 2);
            messages[0].Key.Should().Be("test");
            messages[0].Value.Should().BeEquivalentTo(_domainEventCollection.ToByteArray());
            messages[1].Key.Should().Be("test");
            messages[1].Value.Should().BeEquivalentTo(_domainEventCollection.ToByteArray());
        }
    }
}
