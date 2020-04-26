namespace TradingSystem.Domain.Common
{
    public interface ILogger
    {
        ILogger ForType<T>();
        void Debug(string message);
        void Debug(string format, params object[] formatArgs);
        void Error(string message);
        void Error(string format, params object[] formatArgs);
        void Info(string message);
        void Info(string format, params object[] formatArgs);
        void Warning(string message);
        void Warning(string format, params object[] formatArgs);
    }
}
