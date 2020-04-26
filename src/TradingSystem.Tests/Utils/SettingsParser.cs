using Microsoft.Extensions.Configuration;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Tests.Utils
{
    internal static class SettingsParser
    {
        public static KafkaAdapterSettings KafkaSettings => new ConfigurationBuilder()
                                        .AddJsonFile("appsettings.json")
                                        .Build()
                                        .GetSection("KafkaAdapter")
                                        .Get<KafkaAdapterSettings>();

        public static FileAdapterSettings FileSettings => new ConfigurationBuilder()
                                       .AddJsonFile("appsettings.json")
                                       .Build()
                                       .GetSection("FileAdapter")
                                       .Get<FileAdapterSettings>();

        public static ConsulAdapterSettings ConsulSettings => new ConfigurationBuilder()
                                       .AddJsonFile("appsettings.json")
                                       .Build()
                                       .GetSection("ConsulAdapter")
                                       .Get<ConsulAdapterSettings>();
    }
}
