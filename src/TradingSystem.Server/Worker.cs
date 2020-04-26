using Grpc.Core;
using Grpc.Health.V1;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TradingSystem.Application.Integration.Ports;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Application.Lifecycle.Ports;
using TradingSystem.Application.ReadinessProbe;
using TradingSystem.Application.Service;
using TradingSystem.Domain.Common;
using TradingSystem.Infrastructure.Adapters.Loggers;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Server
{
    internal class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ILeaderElector _leaderElector;
        private readonly OrderBookServiceGRPC _grpcSvc;
        private readonly HealthcheckGRPC _healthCheckGrpcSvc;
        private readonly GrpcAdapterSettings _grpcAdapterSettings;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEventDispatcher<IReadOnlyList<DomainEvent>> _eventPublisher;
        private readonly IEventReceiver<IReadOnlyList<DomainEvent>> _eventReader;
        private readonly IOrderBookService _service;
        private readonly ILifecycleManager _lifecycleManager;
        private readonly IReadinessProbe _probe;

        public Worker(ILogger<Worker> logger, ILeaderElector leaderElector, OrderBookServiceGRPC grpcSvc, HealthcheckGRPC healthCheckGrpcSvc, GrpcAdapterSettings grpcAdapterSettings, ILoggerFactory loggerFactory, IEventDispatcher<IReadOnlyList<DomainEvent>> eventPublisher, IEventReceiver<IReadOnlyList<DomainEvent>> eventReader, IOrderBookService service, ILifecycleManager lifecycleManager, IReadinessProbe probe)
        {
            _logger = logger;
            _leaderElector = leaderElector;
            _grpcSvc = grpcSvc;
            _healthCheckGrpcSvc = healthCheckGrpcSvc;
            _grpcAdapterSettings = grpcAdapterSettings;
            _loggerFactory = loggerFactory;
            _eventPublisher = eventPublisher;
            _eventReader = eventReader;
            _service = service;
            _lifecycleManager = lifecycleManager;
            _probe = probe;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() => Execute(stoppingToken));
        }

        private void Execute(CancellationToken stoppingToken)
        {
            Logger.Instance = new DomainLoggerAdapter<Program>(_loggerFactory);

            while (!_probe.IsReady)
            {
                _logger.LogInformation("Waiting to be ready...");
                Task.Delay(1000, stoppingToken).Wait();
            }

            _logger.LogInformation("Wiring lifecycle manager");
            _lifecycleManager.UseLeaderElection(_leaderElector);

            _eventReader.StoppingToken = stoppingToken;

            _logger.LogInformation("Starting leader elector");
            _leaderElector.Start();

            _logger.LogInformation("Starting GRPC Service");

            GrpcEnvironment.SetLogger(new GrpcLoggerAdapter<OrderBookServiceGRPC>(_loggerFactory));

            var server = new Grpc.Core.Server
            {
                Services = { Health.BindService(_healthCheckGrpcSvc), OrderBookServiceGrpc.BindService(_grpcSvc) },
                Ports = { { _grpcAdapterSettings.Hostname, _grpcAdapterSettings.Port, ServerCredentials.Insecure } },
            };

            server.Start();

            _logger.LogInformation("All done, main thread sleeping...");
            Task.Delay(-1, stoppingToken).Wait();
        }
    }
}
