namespace TradingSystem.Application.Service
{
    public class OrderBookServiceSettings
    {
        public int MillisecondsTimeout { get; set; }

        public override string ToString() => $"MillisecondsTimeout: {MillisecondsTimeout}";
    }
}
