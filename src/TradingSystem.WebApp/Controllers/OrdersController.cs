using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;

namespace TradingSystem.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderBookServiceGrpc.OrderBookServiceGrpcClient _serviceClient;

        public OrdersController(OrderBookServiceGrpc.OrderBookServiceGrpcClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        [HttpPost]
        public async Task<ErrorCodeMsg> Post([FromBody] OrderMsg obj)
        {
            return obj.Type switch
            {
                OrderType.Buy => await _serviceClient.AddBuyOrderAsync(obj),
                OrderType.Sell => await _serviceClient.AddSellOrderAsync(obj),
                _ => throw new ArgumentException("invalid type"),
            };
        }
    }
}