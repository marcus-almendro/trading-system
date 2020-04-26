using Grpc.Core;
using Grpc.Health.V1;
using System;
using System.Threading.Tasks;
using TradingSystem.Application.Lifecycle;

namespace TradingSystem.Infrastructure.Adapters.Service.gRPC
{
    public class HealthcheckGRPC : Health.HealthBase
    {
        private readonly ILifecycleManager _lifecycleManager;

        public HealthcheckGRPC(ILifecycleManager lifecycleManager)
        {
            _lifecycleManager = lifecycleManager;
        }

        public override Task<HealthCheckResponse> Check(HealthCheckRequest request, ServerCallContext context) =>
            Task.FromResult(new HealthCheckResponse()
            {
                Status = _lifecycleManager.CurrentStatus switch
                {
                    Application.Lifecycle.Status.RunningAsLeader => HealthCheckResponse.Types.ServingStatus.Serving,
                    _ => HealthCheckResponse.Types.ServingStatus.NotServing
                }
            });

        public override Task Watch(HealthCheckRequest request, IServerStreamWriter<HealthCheckResponse> responseStream, ServerCallContext context) =>
            throw new NotImplementedException();

    }
}