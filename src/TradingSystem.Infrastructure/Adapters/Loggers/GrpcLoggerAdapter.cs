using Microsoft.Extensions.Logging;
using System;

namespace TradingSystem.Infrastructure.Adapters.Loggers
{
    public class GrpcLoggerAdapter<T> : Grpc.Core.Logging.ILogger
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public GrpcLoggerAdapter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<T>();
            _loggerFactory = loggerFactory;
        }

        public void Debug(string message) => _logger.LogDebug(message);
        public void Debug(string format, params object[] formatArgs) => _logger.LogDebug(format, formatArgs);
        public void Error(string message) => _logger.LogError(message);
        public void Error(string format, params object[] formatArgs) => _logger.LogError(format, formatArgs);
        public void Error(Exception exception, string message) => _logger.LogError(exception, message);
        public Grpc.Core.Logging.ILogger ForType<K>() => new GrpcLoggerAdapter<K>(_loggerFactory);
        public void Info(string message) => _logger.LogInformation(message);
        public void Info(string format, params object[] formatArgs) => _logger.LogInformation(format, formatArgs);
        public void Warning(string message) => _logger.LogWarning(message);
        public void Warning(string format, params object[] formatArgs) => _logger.LogWarning(format, formatArgs);
        public void Warning(Exception exception, string message) => _logger.LogWarning(exception, message);
    }
}
