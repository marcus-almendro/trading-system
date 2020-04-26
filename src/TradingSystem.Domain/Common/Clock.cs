using System;

namespace TradingSystem.Domain.Common
{
    public static class Clock
    {
        public static bool UseTestClock { get; set; }
        public static DateTimeOffset UtcNow => UseTestClock ? TestDateTimeOffset : DateTimeOffset.UtcNow;

        private static DateTimeOffset TestDateTimeOffset => new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
