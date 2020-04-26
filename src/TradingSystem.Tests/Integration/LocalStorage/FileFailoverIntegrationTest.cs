using System.IO;
using TradingSystem.Tests.Utils;

namespace TradingSystem.Tests.Integration.LocalStorage
{
    public class FileFailoverIntegrationTest : FailoverIntegrationTest
    {
        protected override void Setup()
        {
            var settings = SettingsParser.FileSettings;

            if (File.Exists(settings.EventsFileName))
                File.Delete(settings.EventsFileName);
        }

        public override string StorageType => "file";
        public override string LockStrategy => "file";
    }
}
