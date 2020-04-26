using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TradingSystem.Application.Integration.Ports;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.WebApp.State;

namespace TradingSystem.WebApp.Hubs
{
    public class SnapshotHub : Hub
    {
        private string _symbol;
        private readonly IEventReceiver<DomainEventCollection> _receiver;
        private readonly ILogger<SnapshotHub> _logger;
        private readonly Offsets _offsets;

        public SnapshotHub(ILogger<SnapshotHub> logger, IEventReceiver<DomainEventCollection> receiver, Offsets offsets)
        {
            _receiver = receiver;
            _logger = logger;
            _offsets = offsets;
        }

        public void Dispatch(DomainEventCollection evt)
        {
            if (_symbol == evt.Events[0].Symbol && evt.Events[0].EventTypeCase != DomainEventMsg.EventTypeOneofCase.OrderBookCreated)
                Clients.Caller.SendAsync("snp_msg", evt).Wait();
        }

        public async Task GetSnapshot(string symbol)
        {
            using (var scope = _logger.BeginScope($"Snapshot requested for {symbol}"))
            {
                _symbol = symbol;

                _receiver.OnEvent += Dispatch;

                await Groups.AddToGroupAsync(Context.ConnectionId, symbol);
                await Task.Run(() => _receiver.ReceiveUntil(_offsets.Get(symbol)));
                await Clients.Caller.SendAsync("snp_end");
            }
        }
    }
}
