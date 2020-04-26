using TradingSystem.Tests.Utils;

namespace TradingSystem.Tests.Integration.Kafka
{
    public class KafkaFailoverIntegrationTest : FailoverIntegrationTest
    {
        protected override void Setup()
        {
            DockerUtils.StartDockerContainers();
            DockerUtils.WaitTopicCreation(SettingsParser.KafkaSettings);
        }

        public override string StorageType => "kafka";
        public override string LockStrategy => "consul";

        public override void Dispose()
        {
            base.Dispose();
            DockerUtils.CleanupCurrent();
        }
    }
}
