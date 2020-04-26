using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class FileEventDispatcherTest : IDisposable
    {
        private const string _filePath = "test.out";
        private readonly FileEventDispatcher _dispatcher;
        private readonly Mock<IMapper> _mapper;
        private readonly Mock<ILifecycleManager> _lifecycleManager;
        private readonly OrderCreated _orderCreated;
        private readonly DomainEventMsg _orderCreatedMsg;
        private readonly OrderUpdated _orderUpdated;
        private readonly DomainEventMsg _orderUpdatedMsg;
        private readonly IReadOnlyList<DomainEvent> _events;
        private readonly DomainEventCollection _domainEventCollection;

        public FileEventDispatcherTest()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            _mapper = new Mock<IMapper>();
            _lifecycleManager = new Mock<ILifecycleManager>();
            _dispatcher = new FileEventDispatcher(new FileAdapterSettings { EventsFileName = _filePath }, _mapper.Object, _lifecycleManager.Object, new NullLogger<FileEventDispatcher>());
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

            _mapper.Setup(p => p.Map<DomainEventCollection>(_events)).Returns(_domainEventCollection);
        }

        public void Dispose()
        {
            _dispatcher.Dispose();
        }

        [Fact]
        public void ShouldBeAbleToWriteToFile()
        {
            _lifecycleManager.Raise(p => p.StatusChanged += null, Status.BecomingLeader);
            _dispatcher.Dispatch(_events);
            new FileInfo(_filePath).Length.Should().Be(_domainEventCollection.CalculateSize() + 1); //WriteDelimited adds 1 byte
        }

        [Fact]
        public void ShouldBeAbleToWriteToFileMultipleMessages()
        {
            _lifecycleManager.Raise(p => p.StatusChanged += null, Status.BecomingLeader);
            _dispatcher.Dispatch(_events);
            _dispatcher.Dispatch(_events);
            new FileInfo(_filePath).Length.Should().Be(_domainEventCollection.CalculateSize() * 2 + 2);
        }
    }
}
