using AutoMapper;
using System;
using System.Collections.Generic;
using TradingSystem.Application.DTO;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;
using TradingSystem.Domain.Orders;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using OrderType = TradingSystem.Infrastructure.Adapters.Service.gRPC.OrderType;

namespace TradingSystem.Infrastructure.Serialization.AutoMapper
{
    public class InfraMapperProfile : Profile
    {
        public InfraMapperProfile()
        {
            CreateMap<IReadOnlyList<DomainEvent>, DomainEventCollection>().ConvertUsing(typeof(EventConverter));
            CreateMap<DomainEventCollection, IReadOnlyList<DomainEvent>>().ConvertUsing(typeof(EventConverter));
            CreateMap<DateTimeOffset, long>().ConvertUsing(s => s.UtcTicks);
            CreateMap<long, DateTimeOffset>().ConvertUsing(s => new DateTimeOffset(s, TimeSpan.Zero));
            CreateMap<OrderMsg, BuyOrderCommand>().ConstructUsing(p => new BuyOrderCommand()).ReverseMap().ConstructUsing(p => new OrderMsg() { Type = OrderType.Buy });
            CreateMap<OrderMsg, SellOrderCommand>().ConstructUsing(p => new SellOrderCommand()).ReverseMap().ConstructUsing(p => new OrderMsg() { Type = OrderType.Sell });
            CreateMap<OrderMsg, UpdateOrderCommand>().ConstructUsing(p => new UpdateOrderCommand()).ReverseMap().ConstructUsing(p => new OrderMsg() { Type = OrderType.Update });
            CreateMap<OrderMsg, DeleteOrderCommand>().ConstructUsing(p => new DeleteOrderCommand()).ReverseMap().ConstructUsing(p => new OrderMsg() { Type = OrderType.Delete });
            CreateMap<Order, OrderMsg>().ConstructUsing(p => new OrderMsg() { Type = p.OrderType == Domain.Orders.OrderType.Buy ? OrderType.Buy : OrderType.Sell });
            CreateMap<ErrorCodeDTO, ErrorCodeMsg>();
            CreateMap<OrderCreated, DomainEventMsg>()
                .ConstructUsing(p => new DomainEventMsg
                {
                    Symbol = p.Symbol,
                    CreationDate = p.CreationDate.UtcTicks,
                    OrderCreated = new OrderCreatedEventMsg
                    {
                        Id = p.Id,
                        Price = p.Price,
                        Size = p.Size,
                        TraderId = p.TraderId,
                        Type = p.OrderType == Domain.Orders.OrderType.Buy ? OrderType.Buy : OrderType.Sell
                    }
                }).ReverseMap().ConstructUsing(p => new OrderCreated(GetOrder(p.Symbol, p.OrderCreated.Id, p.OrderCreated.Price, p.OrderCreated.Size, p.OrderCreated.TraderId, p.OrderCreated.Type)));
            CreateMap<OrderUpdated, DomainEventMsg>()
                .ConstructUsing(p => new DomainEventMsg
                {
                    Symbol = p.Symbol,
                    CreationDate = p.CreationDate.UtcTicks,
                    OrderUpdated = new OrderUpdatedEventMsg
                    {
                        Id = p.Id,
                        Price = p.Price,
                        Size = p.Size,
                        TraderId = p.TraderId,
                        Type = p.OrderType == Domain.Orders.OrderType.Buy ? OrderType.Buy : OrderType.Sell
                    }
                }).ReverseMap().ConstructUsing(p => new OrderUpdated(GetOrder(p.Symbol, p.OrderUpdated.Id, p.OrderUpdated.Price, p.OrderUpdated.Size, p.OrderUpdated.TraderId, p.OrderUpdated.Type)));
            CreateMap<OrderDeleted, DomainEventMsg>()
                .ConstructUsing(p => new DomainEventMsg
                {
                    Symbol = p.Symbol,
                    CreationDate = p.CreationDate.UtcTicks,
                    OrderDeleted = new OrderDeletedEventMsg
                    {
                        Id = p.Id,
                        Price = p.Price,
                        Size = p.Size,
                        TraderId = p.TraderId,
                        Type = p.OrderType == Domain.Orders.OrderType.Buy ? OrderType.Buy : OrderType.Sell
                    }
                }).ReverseMap().ConstructUsing(p => new OrderDeleted(GetOrder(p.Symbol, p.OrderDeleted.Id, p.OrderDeleted.Price, p.OrderDeleted.Size, p.OrderDeleted.TraderId, p.OrderDeleted.Type)));
            CreateMap<Trade, DomainEventMsg>()
                .ConstructUsing(p => new DomainEventMsg
                {
                    Symbol = p.Symbol,
                    CreationDate = p.CreationDate.UtcTicks,
                    Trade = new TradeEventMsg
                    {
                        TakerOrderId = p.TakerOrderId,
                        TakenOrderId = p.TakenOrderId,
                        Price = p.Price,
                        ExecutedSize = p.ExecutedSize,
                    }
                }).ReverseMap().ConstructUsing(p => new Trade(p.Symbol, p.Trade.TakerOrderId, p.Trade.TakenOrderId, p.Trade.Price, p.Trade.ExecutedSize));
            CreateMap<OrderBookCreated, DomainEventMsg>()
                .ConstructUsing(p => new DomainEventMsg
                {
                    Symbol = p.Symbol,
                    CreationDate = p.CreationDate.UtcTicks,
                    OrderBookCreated = new OrderBookCreatedEventMsg()
                }).ReverseMap().ConstructUsing(p => new OrderBookCreated(p.Symbol));
        }

        private static Order GetOrder(string symbol, long id, long price, long size, int traderId, OrderType type)
        {
            switch (type)
            {
                case OrderType.Buy:
                    return new Order(symbol, id, Domain.Orders.OrderType.Buy, price, size, traderId);
                case OrderType.Sell:
                    return new Order(symbol, id, Domain.Orders.OrderType.Sell, price, size, traderId);
                default:
                    throw new InvalidOperationException("Invalid order type");
            }
        }
    }
}
