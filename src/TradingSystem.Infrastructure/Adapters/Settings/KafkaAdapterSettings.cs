namespace TradingSystem.Infrastructure.Adapters.Settings
{
    public class KafkaAdapterSettings
    {
        public string BrokerList { get; set; }
        public string EventsTopic { get; set; }

        public string FirstBrokerHostname => BrokerList.Split(',')[0].Split(':')[0];
        public int FirstBrokerPort => int.Parse(BrokerList.Split(',')[0].Split(':')[1]);

        public override string ToString() => $"BrokerList: {BrokerList}, EventsTopic: {EventsTopic}";
    }
}
