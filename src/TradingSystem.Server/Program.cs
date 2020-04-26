using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TradingSystem.Application.Integration.Ports;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Application.Lifecycle.Ports;
using TradingSystem.Application.ReadinessProbe;
using TradingSystem.Application.Service;
using TradingSystem.Application.Service.Decorators;
using TradingSystem.Domain.Common;
using TradingSystem.Domain.Orders.Books;
using TradingSystem.Infrastructure.Adapters.Integration.Kafka;
using TradingSystem.Infrastructure.Adapters.Integration.LocalStorage;
using TradingSystem.Infrastructure.Adapters.Lifecycle.Consul;
using TradingSystem.Infrastructure.Adapters.Lifecycle.LocalStorage;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Adapters.Settings;
using TradingSystem.Infrastructure.ReadinessProbe;
using TradingSystem.Infrastructure.Repository;
using TradingSystem.Infrastructure.Serialization.AutoMapper;
using TradingSystem.Infrastructure.Utils;

namespace TradingSystem.Server
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if ((Environment.GetEnvironmentVariable("Main__Debug") ?? "").ToLower() == "true")
                Debugger.Launch();

            var host = CreateHostBuilder(args).Build();

            var logger = host.Services.GetService<ILogger<Program>>();
            logger.LogInformation("MainSettings: " + host.Services.GetService<MainSettings>());
            logger.LogInformation("OrderBookServiceSettings: " + host.Services.GetService<OrderBookServiceSettings>());
            logger.LogInformation("GrpcAdapterSettings: " + host.Services.GetService<GrpcAdapterSettings>());
            logger.LogInformation("FileAdapterSettings: " + host.Services.GetService<FileAdapterSettings>());
            logger.LogInformation("KafkaAdapterSettings: " + host.Services.GetService<KafkaAdapterSettings>());
            logger.LogInformation("ConsulAdapterSettings: " + host.Services.GetService<ConsulAdapterSettings>());
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // settings
                    var settings = hostContext.Configuration.GetSection("Main").Get<MainSettings>();
                    services.AddSingleton(settings);
                    services.AddSingleton(hostContext.Configuration.GetSection("OrderBookService").Get<OrderBookServiceSettings>());
                    services.AddSingleton(hostContext.Configuration.GetSection("GrpcAdapter").Get<GrpcAdapterSettings>());
                    services.AddSingleton(hostContext.Configuration.GetSection("KafkaAdapter").Get<KafkaAdapterSettings>());
                    services.AddSingleton(hostContext.Configuration.GetSection("FileAdapter").Get<FileAdapterSettings>());
                    services.AddSingleton(hostContext.Configuration.GetSection("ConsulAdapter").Get<ConsulAdapterSettings>());

                    // application
                    services.AddAutoMapper(typeof(InfraMapperProfile));
                    services.AddSingleton<IOrderBookRepository, InMemoryOrderBookRepository>();
                    services.AddSingleton<ILifecycleManager, LifecycleManager>();

                    // IOrderBookService -> Logged -> Authorized -> Synchronized -> OrderBookService
                    services.AddSingleton<IOrderBookService, OrderBookService>();
                    services.Decorate<IOrderBookService, SynchronizedOrderBookService>();
                    services.Decorate<IOrderBookService, AuthorizedOrderBookService>();
                    services.Decorate<IOrderBookService, LoggedOrderBookService>();

                    // infrastructure
                    if (settings.Storage == MainSettings.StorageType.Kafka)
                    {
                        services.AddSingleton<IEventDispatcher<IReadOnlyList<DomainEvent>>, KafkaEventDispatcher>();
                        services.AddSingleton<IEventReceiver<IReadOnlyList<DomainEvent>>, KafkaEventReceiver<IReadOnlyList<DomainEvent>>>();
                        services.AddSingleton<IReadinessProbe, KafkaReadinessProbe>();
                    }
                    else
                    {
                        services.AddSingleton<IEventDispatcher<IReadOnlyList<DomainEvent>>, FileEventDispatcher>();
                        services.AddSingleton<IEventReceiver<IReadOnlyList<DomainEvent>>, FileEventReceiver<IReadOnlyList<DomainEvent>>>();
                        services.AddSingleton<IReadinessProbe, FileReadinessProbe>();
                    }

                    if (settings.LockStrategy == MainSettings.LockType.Consul)
                    {
                        services.AddSingleton<ILeaderElector, ConsulLockLeaderElector>();
                    }
                    else
                    {
                        services.AddSingleton<ILeaderElector, FileLockLeaderElector>();
                    }

                    services.AddSingleton<OrderBookServiceGRPC>();
                    services.AddSingleton<HealthcheckGRPC>();
                    services.AddHostedService<Worker>();

                });
        }

    }
}
