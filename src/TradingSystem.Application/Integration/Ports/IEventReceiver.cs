using System;
using System.Threading;

namespace TradingSystem.Application.Integration.Ports
{
    public interface IEventReceiver<T>
    {
        CancellationToken StoppingToken { set; }
        event Action<T> OnEvent;
        void ReceiveUntil(long maxOffset);
    }
}