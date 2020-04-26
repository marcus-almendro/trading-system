namespace TradingSystem.Infrastructure.Adapters.Settings
{
    public class GrpcAdapterSettings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }

        public override string ToString() => $"Hostname: {Hostname}, Port: {Port}";
    }
}
