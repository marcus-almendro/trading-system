using Microsoft.Extensions.Logging;
using System;
using System.IO;
using TradingSystem.Application.ReadinessProbe;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Infrastructure.ReadinessProbe
{
    public class FileReadinessProbe : IReadinessProbe
    {
        private readonly FileAdapterSettings _settings;
        private readonly ILogger<FileReadinessProbe> _logger;

        public FileReadinessProbe(FileAdapterSettings settings, ILogger<FileReadinessProbe> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public bool IsReady => CanWriteAtFolder(_settings.EventsFileName) && CanWriteAtFolder(_settings.LockFileName);

        public bool CanWriteAtFolder(string file)
        {
            try
            {
                var dummyFile = Path.Combine(Path.GetDirectoryName(file), Guid.NewGuid().ToString());
                _logger.LogInformation($"Testing write at {dummyFile}");
                File.WriteAllText(dummyFile, "1");
                File.Delete(dummyFile);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
