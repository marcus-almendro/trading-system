using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using TradingSystem.Application.Integration.Ports;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.WebApp.State;

namespace TradingSystem.WebApp.Hubs
{
    public class IncrementalHub : IHostedService
    {
        private readonly IEventReceiver<DomainEventCollection> _receiver;
        private readonly IHubContext<SnapshotHub> _hubContext;
        private readonly Offsets _offsets;
        private readonly ILifecycleManager _lifecycleManager;

        public IncrementalHub(IEventReceiver<DomainEventCollection> receiver, IHubContext<SnapshotHub> hubContext, Offsets offsets, ILifecycleManager lifecycleManager)
        {
            _receiver = receiver;
            _hubContext = hubContext;
            _offsets = offsets;
            _lifecycleManager = lifecycleManager;
        }

        public void Dispatch(DomainEventCollection evt)
        {
            _offsets.Set(evt.Events[0].Symbol, evt.Offset);
            if (evt.Events[0].EventTypeCase != DomainEventMsg.EventTypeOneofCase.OrderBookCreated)
                _hubContext.Clients.Group(evt.Events[0].Symbol).SendAsync("inc_msg", evt).Wait();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _receiver.OnEvent += Dispatch;
            _lifecycleManager.BecomeFollower();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
