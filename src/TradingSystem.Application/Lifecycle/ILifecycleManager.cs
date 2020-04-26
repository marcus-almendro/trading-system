using System;
using TradingSystem.Application.Lifecycle.Ports;

namespace TradingSystem.Application.Lifecycle
{
    public interface ILifecycleManager
    {
        bool IsLeader { get => CurrentStatus == Status.RunningAsLeader; }
        DateTimeOffset LastStatusChange { get; }
        Status CurrentStatus { get; }
        event Action<Status> StatusChanged;

        void BecomeLeader();
        void BecomeFollower();
        void UseLeaderElection(ILeaderElector leaderElector);
        void Shutdown();
    }
}
