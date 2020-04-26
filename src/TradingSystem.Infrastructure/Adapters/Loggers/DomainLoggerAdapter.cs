using Microsoft.Extensions.Logging;

namespace TradingSystem.Infrastructure.Adapters.Loggers
{
    public class DomainLoggerAdapter<T> : Domain.Common.ILogger
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public DomainLoggerAdapter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<T>();
            _loggerFactory = loggerFactory;
        }

        public void Debug(string message) => _logger.LogDebug(message);
        public void Debug(string format, params object[] formatArgs) => _logger.LogDebug(format, formatArgs);
        public void Error(string message) => _logger.LogError(message);
        public void Error(string format, params object[] formatArgs) => _logger.LogError(format, formatArgs);
        public Domain.Common.ILogger ForType<K>() => new DomainLoggerAdapter<K>(_loggerFactory);
        public void Info(string message) => _logger.LogInformation(message);
        public void Info(string format, params object[] formatArgs) => _logger.LogInformation(format, formatArgs);
        public void Warning(string message) => _logger.LogWarning(message);
        public void Warning(string format, params object[] formatArgs) => _logger.LogWarning(format, formatArgs);
    }
}
