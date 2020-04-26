using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;

namespace TradingSystem.WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderBooksController : ControllerBase
    {
        private readonly OrderBookServiceGrpc.OrderBookServiceGrpcClient _serviceClient;

        public OrderBooksController(OrderBookServiceGrpc.OrderBookServiceGrpcClient serviceClient)
        {
            _serviceClient = serviceClient;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            return (await _serviceClient.GetAllOrderBooksAsync(new GetAllOrderBooksRequest())).Symbols;
        }

        [HttpPost]
        public async Task<ErrorCodeMsg> Post([FromBody] NewOrderBook obj)
        {
            return await _serviceClient.AddOrderBookAsync(obj);
        }
    }
}
