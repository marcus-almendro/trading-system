using System;

namespace TradingSystem.Infrastructure.Adapters.Settings
{
    public class ConsulAdapterSettings
    {
        public Uri Address { get; set; }
        public string Key { get; set; }
        public int SessionTTL { get; set; }

        public override string ToString() => $"Address: {Address}, Key: {Key}, TTL: {SessionTTL}";
    }
}
