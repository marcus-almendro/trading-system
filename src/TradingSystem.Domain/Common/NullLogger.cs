﻿namespace TradingSystem.Domain.Common
{
    internal class NullLogger : ILogger
    {
        public void Debug(string message) { }
        public void Debug(string format, params object[] formatArgs) { }
        public void Error(string message) { }
        public void Error(string format, params object[] formatArgs) { }
        public ILogger ForType<T>() => this;
        public void Info(string message) { }
        public void Info(string format, params object[] formatArgs) { }
        public void Warning(string message) { }
        public void Warning(string format, params object[] formatArgs) { }
    }
}
