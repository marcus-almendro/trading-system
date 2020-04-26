using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Sockets;
using TradingSystem.Application.ReadinessProbe;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Infrastructure.ReadinessProbe
{
    public class KafkaReadinessProbe : IReadinessProbe
    {
        private readonly KafkaAdapterSettings _settings;
        private readonly ILogger<KafkaReadinessProbe> _logger;

        public KafkaReadinessProbe(KafkaAdapterSettings settings, ILogger<KafkaReadinessProbe> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public bool IsReady => IsPortOpen() && AllTopicsExists();

        private bool IsPortOpen()
        {
            using (var tcpClient = new TcpClient())
            {
                try
                {
                    _logger.LogInformation($"Testing connection to {_settings.FirstBrokerHostname}:{_settings.FirstBrokerPort}");
                    tcpClient.Connect(_settings.FirstBrokerHostname, _settings.FirstBrokerPort);
                    return true;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error TCP");
                    return false;
                }
            }
        }

        private bool AllTopicsExists()
        {
            try
            {
                _logger.LogInformation($"Checking if topics exists");
                var topics = new AdminClientBuilder(new AdminClientConfig()
                {
                    BootstrapServers = _settings.BrokerList,
                }).Build().GetMetadata(TimeSpan.FromSeconds(5)).Topics;

                topics.ForEach(t => _logger.LogInformation($"Found topic: {t}"));

                var count = topics.Join(new[]
                {
                    _settings.EventsTopic,
                }, t => t.Topic, t => t, (a, b) => b).Count();

                return count == 1;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Topics");
                return false;
            }

        }
    }
}
