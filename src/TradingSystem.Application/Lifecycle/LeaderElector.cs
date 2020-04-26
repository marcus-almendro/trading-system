using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using TradingSystem.Application.Lifecycle.Ports;
using Timer = System.Timers.Timer;

namespace TradingSystem.Application.Lifecycle
{
    public abstract class LeaderElector : ILeaderElector
    {
        private readonly ILogger _logger;
        private readonly ILifecycleManager _lifecycleManager;
        private Timer _timer;

        public LeaderElector(ILogger logger, ILifecycleManager lifecycleManager)
        {
            _logger = logger;
            _lifecycleManager = lifecycleManager;
        }

        public event Action OnBecomeLeader;
        public event Action OnBecomeFollower;

        protected abstract bool HasLock { get; }
        protected abstract void GetLockSync();

        public void Start()
        {
            _logger.LogInformation("Starting timer");
            if (_timer != null)
                throw new InvalidProgramException("Already started leader elector");

            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) => TryGetLock();
            _timer.AutoReset = false;

            _logger.LogInformation("Starting as follower...");
            OnBecomeFollower?.Invoke();

            _timer.Start();
        }

        private void TryGetLock()
        {
            try
            {
                if (!HasLock)
                {
                    if (_lifecycleManager.IsLeader)
                    {
                        _logger.LogError("Lost leadership!?");
                        _lifecycleManager.Shutdown();
                    }
                    else
                    {
                        _logger.LogDebug("Trying to acquire lock...");
                        GetLockSync();

                        if (HasLock)
                        {
                            _logger.LogInformation("Got lock! Becoming leader...");
                            OnBecomeLeader?.Invoke();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!HasLock)
                    _logger.LogTrace(e, "Cannot get lock");
                else
                    ThreadPool.QueueUserWorkItem(_ => { throw new Exception("Exception when invoking OnBecomeLeader!", e); });
            }
            finally
            {
                _timer.Start();
            }
        }
    }
}
