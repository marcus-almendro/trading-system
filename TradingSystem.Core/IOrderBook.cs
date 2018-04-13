using System;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("TradingSystem.Tests")]

namespace TradingSystem.Core
{
    public interface IOrderBook
    {
        event EventHandler<Message> OnMessage;
        ErrorCode Enter(Order order);
    }
}