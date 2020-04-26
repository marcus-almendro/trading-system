namespace TradingSystem.WebApp
{
    internal class AdapterConfig
    {
        public StorageType Storage { get; set; }

        public override string ToString() => $"Storage: {Storage}";

        internal enum StorageType
        {
            Kafka,
            File
        }
    }
}
