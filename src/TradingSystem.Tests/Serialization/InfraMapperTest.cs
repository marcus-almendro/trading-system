using AutoMapper;
using FluentAssertions;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;
using TradingSystem.Domain.Orders;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Serialization.AutoMapper;
using Xunit;

namespace TradingSystem.Tests.Serialization
{
    public class InfraMapperTest
    {
        private readonly IMapper _mapper;

        public InfraMapperTest()
        {
            Clock.UseTestClock = true;
            _mapper = new MapperConfiguration(cfg => cfg.AddProfile<InfraMapperProfile>()).CreateMapper();
        }

        [Fact]
        public void OrderCreatedEvent()
        {
            var buyOrderCreated = new OrderCreated(new Order("test", 0, TradingSystem.Domain.Orders.OrderType.Buy, 100, 100, 0));
            var buyOrderCreatedMsg = new DomainEventMsg()
            {
                CreationDate = buyOrderCreated.CreationDate.UtcTicks,
                OrderCreated = new OrderCreatedEventMsg { Id = 0, Price = 100, Size = 100, TraderId = 0, Type = Infrastructure.Adapters.Service.gRPC.OrderType.Buy },
                Symbol = "test"
            };
            _mapper.Map<DomainEventMsg>(buyOrderCreated).Should().BeEquivalentTo(buyOrderCreatedMsg);
            _mapper.Map<OrderCreated>(buyOrderCreatedMsg).Should().BeEquivalentTo(buyOrderCreated);
        }

        [Fact]
        public void OrderUpdatedEvent()
        {
            var buyOrderUpdated = new OrderUpdated(new Order("test", 0, TradingSystem.Domain.Orders.OrderType.Buy, 100, 100, 0));
            var buyOrderUpdatedMsg = new DomainEventMsg()
            {
                CreationDate = buyOrderUpdated.CreationDate.UtcTicks,
                OrderUpdated = new OrderUpdatedEventMsg { Id = 0, Price = 100, Size = 100, TraderId = 0, Type = Infrastructure.Adapters.Service.gRPC.OrderType.Buy },
                Symbol = "test"
            };
            _mapper.Map<DomainEventMsg>(buyOrderUpdated).Should().BeEquivalentTo(buyOrderUpdatedMsg);
            _mapper.Map<OrderUpdated>(buyOrderUpdatedMsg).Should().BeEquivalentTo(buyOrderUpdated);
        }

        [Fact]
        public void OrderDeletedEvent()
        {
            var buyOrderDeleted = new OrderDeleted(new Order("test", 0, TradingSystem.Domain.Orders.OrderType.Buy, 100, 100, 0));
            var buyOrderDeletedMsg = new DomainEventMsg()
            {
                CreationDate = buyOrderDeleted.CreationDate.UtcTicks,
                OrderDeleted = new OrderDeletedEventMsg { Id = 0, Price = 100, Size = 100, TraderId = 0, Type = Infrastructure.Adapters.Service.gRPC.OrderType.Buy },
                Symbol = "test"
            };
            _mapper.Map<DomainEventMsg>(buyOrderDeleted).Should().BeEquivalentTo(buyOrderDeletedMsg);
            _mapper.Map<OrderDeleted>(buyOrderDeletedMsg).Should().BeEquivalentTo(buyOrderDeleted);
        }

        [Fact]
        public void TradeEvent()
        {
            var trade = new Trade("test", 0, 1, 100, 100);
            var tradeMsg = new DomainEventMsg()
            {
                CreationDate = trade.CreationDate.UtcTicks,
                Trade = new TradeEventMsg
                {
                    TakerOrderId = 0,
                    TakenOrderId = 1,
                    Price = 100,
                    ExecutedSize = 100
                },
                Symbol = "test"
            };
            _mapper.Map<DomainEventMsg>(trade).Should().BeEquivalentTo(tradeMsg);
            _mapper.Map<Trade>(tradeMsg).Should().BeEquivalentTo(trade);
        }

        [Fact]
        public void Commands()
        {
            var buyOrderCommand = new BuyOrderCommand() { Price = 100, Size = 200, Symbol = "test", TraderId = 1 };
            var buyOrderMsg = new OrderMsg() { Price = 100, Size = 200, Symbol = "test", TraderId = 1, Type = Infrastructure.Adapters.Service.gRPC.OrderType.Buy };
            _mapper.Map<OrderMsg>(buyOrderCommand).Should().BeEquivalentTo(buyOrderMsg);
            _mapper.Map<BuyOrderCommand>(buyOrderMsg).Should().BeEquivalentTo(buyOrderCommand);

            var sellOrderCommand = new SellOrderCommand() { Price = 100, Size = 200, Symbol = "test", TraderId = 1 };
            var sellOrderMsg = new OrderMsg() { Price = 100, Size = 200, Symbol = "test", TraderId = 1, Type = Infrastructure.Adapters.Service.gRPC.OrderType.Sell };
            _mapper.Map<OrderMsg>(sellOrderCommand).Should().BeEquivalentTo(sellOrderMsg);
            _mapper.Map<SellOrderCommand>(sellOrderMsg).Should().BeEquivalentTo(sellOrderCommand);

            var updateOrderCommand = new UpdateOrderCommand() { Id = 3, Size = 200, Symbol = "test" };
            var updateOrderMsg = new OrderMsg() { Id = 3, Size = 200, Symbol = "test", Type = Infrastructure.Adapters.Service.gRPC.OrderType.Update };
            _mapper.Map<OrderMsg>(updateOrderCommand).Should().BeEquivalentTo(updateOrderMsg);
            _mapper.Map<UpdateOrderCommand>(updateOrderMsg).Should().BeEquivalentTo(updateOrderCommand);

            var deleteOrderCommand = new DeleteOrderCommand() { Id = 4, Symbol = "test" };
            var deleteOrderMsg = new OrderMsg() { Id = 4, Symbol = "test", Type = Infrastructure.Adapters.Service.gRPC.OrderType.Delete };
            _mapper.Map<OrderMsg>(deleteOrderCommand).Should().BeEquivalentTo(deleteOrderMsg);
            _mapper.Map<DeleteOrderCommand>(deleteOrderMsg).Should().BeEquivalentTo(deleteOrderCommand);
        }
    }
}
