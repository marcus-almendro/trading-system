using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TradingSystem.Application.Integration.Ports;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Infrastructure.Adapters.Integration.Kafka;
using TradingSystem.Infrastructure.Adapters.Integration.LocalStorage;
using TradingSystem.Infrastructure.Adapters.Service.gRPC;
using TradingSystem.Infrastructure.Adapters.Settings;
using TradingSystem.Infrastructure.Serialization.AutoMapper;
using TradingSystem.WebApp.Hubs;
using TradingSystem.WebApp.State;

namespace TradingSystem.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "dist";
            });
            services.AddSignalR();
            services.AddHostedService<IncrementalHub>();
            services.AddAutoMapper(cfg => cfg.AddProfile<InfraMapperProfile>(), typeof(InfraMapperProfile));
            services.AddSingleton<Offsets>();
            services.AddSingleton<ILifecycleManager, LifecycleManager>();

            var grpcSettings = Configuration.GetSection("GrpcAdapter").Get<GrpcAdapterSettings>();
            services.AddGrpcClient<OrderBookServiceGrpc.OrderBookServiceGrpcClient>(opt => opt.Address = new Uri($"http://{grpcSettings.Hostname}:{grpcSettings.Port}"));

            if (Configuration.GetSection("AdapterConfig").Get<AdapterConfig>().Storage == AdapterConfig.StorageType.File)
            {
                services.AddSingleton(Configuration.GetSection("FileAdapter").Get<FileAdapterSettings>());
                services.AddTransient<IEventReceiver<DomainEventCollection>, FileEventReceiver<DomainEventCollection>>();
            }
            else
            {
                services.AddSingleton(Configuration.GetSection("KafkaAdapter").Get<KafkaAdapterSettings>());
                services.AddTransient<IEventReceiver<DomainEventCollection>, KafkaEventReceiver<DomainEventCollection>>();
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapHub<SnapshotHub>("/events");
            });

            app.UseSpa(spa =>
            {
                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
                }
            });
        }
    }
}
