using AutoMapper;
using Grpc.Core;
using System.Linq;
using System.Threading.Tasks;
using TradingSystem.Application.DTO.Commands;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Application.Service;

namespace TradingSystem.Infrastructure.Adapters.Service.gRPC
{
    public class OrderBookServiceGRPC : OrderBookServiceGrpc.OrderBookServiceGrpcBase
    {
        private readonly IOrderBookService _orderBookService;
        private readonly ILifecycleManager _lifecycleManager;
        private readonly IMapper _mapper;

        public OrderBookServiceGRPC(ILifecycleManager lifecycleManager, IOrderBookService orderBookService, IMapper mapper)
        {
            _lifecycleManager = lifecycleManager;
            _orderBookService = orderBookService;
            _mapper = mapper;
        }

        public override Task<ErrorCodeMsg> AddOrderBook(NewOrderBook orderBook, ServerCallContext context) =>
            Task.FromResult(_mapper.Map<ErrorCodeMsg>(_orderBookService.AddOrderBook(orderBook.Symbol)));

        public override Task<ErrorCodeMsg> AddBuyOrder(OrderMsg orderMsg, ServerCallContext context) =>
            Task.FromResult(_mapper.Map<ErrorCodeMsg>(_orderBookService.AddBuyOrder(_mapper.Map<BuyOrderCommand>(orderMsg))));

        public override Task<ErrorCodeMsg> AddSellOrder(OrderMsg orderMsg, ServerCallContext context) =>
            Task.FromResult(_mapper.Map<ErrorCodeMsg>(_orderBookService.AddSellOrder(_mapper.Map<SellOrderCommand>(orderMsg))));

        public override Task<ErrorCodeMsg> UpdateOrder(OrderMsg orderMsg, ServerCallContext context) =>
            Task.FromResult(_mapper.Map<ErrorCodeMsg>(_orderBookService.UpdateOrder(_mapper.Map<UpdateOrderCommand>(orderMsg))));

        public override Task<ErrorCodeMsg> DeleteOrder(OrderMsg orderMsg, ServerCallContext context) =>
            Task.FromResult(_mapper.Map<ErrorCodeMsg>(_orderBookService.DeleteOrder(_mapper.Map<DeleteOrderCommand>(orderMsg))));

        public override Task<OrderBookCollection> GetAllOrderBooks(GetAllOrderBooksRequest request, ServerCallContext context)
        {
            var r = new OrderBookCollection();
            r.Symbols.AddRange(_orderBookService.GetAllOrderBooks());
            return Task.FromResult(r);
        }

        public override Task<ServiceStatus> Status(GetStatus getStatus, ServerCallContext context)
        {
            var status = new ServiceStatus()
            {
                LastStatusChange = _lifecycleManager.LastStatusChange.UtcTicks,
                IsLeader = _lifecycleManager.IsLeader,
                TotalOrders = _orderBookService.TotalOrders(getStatus.Symbol)
            };

            if (getStatus.IncludeDump)
                status.AllOrders.AddRange(_orderBookService.UnsafeDump(getStatus.Symbol).Select(o => _mapper.Map<OrderMsg>(o)));

            return Task.FromResult(status);
        }

    }
}