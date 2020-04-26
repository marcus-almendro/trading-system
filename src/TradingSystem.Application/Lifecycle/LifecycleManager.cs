using Microsoft.Extensions.Logging;
using System;
using TradingSystem.Application.Lifecycle.Ports;
using TradingSystem.Domain.Common;

namespace TradingSystem.Application.Lifecycle
{
    public class LifecycleManager : ILifecycleManager
    {
        private Status _currentStatus;
        private readonly ILogger<LifecycleManager> _logger;

        public LifecycleManager(ILogger<LifecycleManager> logger)
        {
            _currentStatus = Status.Uninitialized;
            _logger = logger;
        }

        public event Action<Status> StatusChanged;

        public Status CurrentStatus
        {
            get => _currentStatus;
            private set
            {
                _currentStatus = value;
                LastStatusChange = Clock.UtcNow;
                StatusChanged?.Invoke(_currentStatus);
                _logger.LogInformation($"Status changed to {_currentStatus}");
            }
        }

        public DateTimeOffset LastStatusChange { get; private set; }

        public void UseLeaderElection(ILeaderElector leaderElector)
        {
            leaderElector.OnBecomeLeader += BecomeLeader;
            leaderElector.OnBecomeFollower += BecomeFollower;
        }
        public void BecomeLeader() => ChangeStatus(Status.RunningAsLeader);
        public void BecomeFollower() => ChangeStatus(Status.RunningAsFollower);

        private void ChangeStatus(Status newStatus)
        {
            switch (CurrentStatus)
            {
                case Status.Uninitialized:
                    switch (newStatus)
                    {
                        case Status.RunningAsFollower:
                            CurrentStatus = Status.StartingAsFollower;
                            CurrentStatus = Status.RunningAsFollower;
                            break;
                        default: throw new InvalidOperationException();
                    }
                    break;
                case Status.RunningAsFollower:
                    switch (newStatus)
                    {
                        case Status.RunningAsLeader:
                            CurrentStatus = Status.BecomingLeader;
                            CurrentStatus = Status.RunningAsLeader;
                            break;
                        default: throw new InvalidOperationException();
                    }
                    break;
                default: throw new InvalidOperationException();
            }
        }

        public void Shutdown() => Environment.Exit(321);
    }
}
