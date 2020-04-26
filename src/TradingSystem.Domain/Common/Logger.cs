namespace TradingSystem.Domain.Common
{
    public static class Logger
    {
        public static ILogger Instance { get; set; } = new NullLogger();

        public static ILogger ForType<T>() => Instance.ForType<T>();
    }
}
