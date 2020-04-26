using System;

namespace TradingSystem.Application.Lifecycle.Ports
{
    public interface ILeaderElector
    {
        event Action OnBecomeLeader;
        event Action OnBecomeFollower;
        void Start();
    }
}
