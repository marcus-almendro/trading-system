using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;
using TradingSystem.Domain.Orders;
using TradingSystem.Infrastructure.Adapters.Integration.LocalStorage;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Adapters.Settings;
using Xunit;

namespace TradingSystem.Tests.Integration.LocalStorage
{
    public class FileEventReceiverTest
    {
        private const string _filePath = "test_read.out";
        private readonly FileEventReceiver<IReadOnlyList<DomainEvent>> _receiver;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILifecycleManager> _lifecycleManager;
        private readonly OrderCreated _orderCreated;
        private readonly DomainEventMsg _orderCreatedMsg;
        private readonly OrderUpdated _orderUpdated;
        private readonly DomainEventMsg _orderUpdatedMsg;
        private readonly IReadOnlyList<DomainEvent> _events;
        private readonly DomainEventCollection _domainEventCollection;

        public FileEventReceiverTest()
        {
            _mapper = new Mock<IMapper>();
            _lifecycleManager = new Mock<ILifecycleManager>();
            _receiver = new FileEventReceiver<IReadOnlyList<DomainEvent>>(new FileAdapterSettings { EventsFileName = _filePath }, _mapper.Object, _lifecycleManager.Object, new NullLogger<FileEventReceiver<IReadOnlyList<DomainEvent>>>());
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
                    TraderId = 1,
                    Type = Infrastructure.Adapters.Service.gRPC.OrderType.Buy
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
                    TraderId = 1,
                    Type = Infrastructure.Adapters.Service.gRPC.OrderType.Buy
                }
            };
            _events = new List<DomainEvent> { _orderCreated, _orderUpdated }.AsReadOnly();
            _domainEventCollection = new DomainEventCollection();
            _domainEventCollection.Events.Add(_orderCreatedMsg);
            _domainEventCollection.Events.Add(_orderUpdatedMsg);

            var firstMsg = _domainEventCollection.Clone();
            firstMsg.Offset = 33;
            _mapper.Setup(p => p.Map<IReadOnlyList<DomainEvent>>(firstMsg)).Returns(_events);

            var secondMsg = _domainEventCollection.Clone();
            secondMsg.Offset = 66;
            _mapper.Setup(p => p.Map<IReadOnlyList<DomainEvent>>(secondMsg)).Returns(_events);
        }

        [Fact]
        public void ShouldBeAbleToReadMultipleMessages()
        {
            var list = new List<IReadOnlyList<DomainEvent>>();
            _receiver.OnEvent += (msg) => list.Add(msg);
            _lifecycleManager.Raise(p => p.StatusChanged += null, Status.StartingAsFollower);
            for (var i = 0; i < 10 && _receiver.TotalMessages != 2; i++)
                Task.Delay(100).Wait();

            list.Should().HaveCount(2);
            list[0].Should().BeEquivalentTo(_events);
            list[1].Should().BeEquivalentTo(_events);
        }
    }
}
