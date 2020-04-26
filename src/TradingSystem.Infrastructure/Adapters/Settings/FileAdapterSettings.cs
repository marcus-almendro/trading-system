namespace TradingSystem.Infrastructure.Adapters.Settings
{
    public class FileAdapterSettings
    {
        public string LockFileName { get; set; }
        public string EventsFileName { get; set; }

        public override string ToString() => $"LockFileName: {LockFileName}, EventsFileName: {EventsFileName}";
    }
}
