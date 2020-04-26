namespace TradingSystem.Server
{
    internal class MainSettings
    {
        public StorageType Storage { get; set; }
        public LockType LockStrategy { get; set; }
        public bool Debug { get; set; }

        public override string ToString() => $"Storage: {Storage}, LockStrategy: {LockStrategy}, Debug: {Debug}";

        internal enum StorageType
        {
            Kafka,
            File
        }

        internal enum LockType
        {
            Consul,
            File
        }
    }
}
