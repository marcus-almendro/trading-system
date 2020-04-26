using Consul;
using Microsoft.Extensions.Logging;
using System;
using TradingSystem.Application.Lifecycle;
using TradingSystem.Infrastructure.Adapters.Settings;

namespace TradingSystem.Infrastructure.Adapters.Lifecycle.Consul
{
    public class ConsulLockLeaderElector : LeaderElector
    {
        private readonly ConsulClient _client;
        private readonly ConsulAdapterSettings _settings;
        private IDistributedLock _lock;

        public ConsulLockLeaderElector(ILogger<ConsulLockLeaderElector> logger, ILifecycleManager lifecycleManager, ConsulAdapterSettings settings)
            : base(logger, lifecycleManager)
        {
            _client = new ConsulClient(cfg => cfg.Address = settings.Address);
            _settings = settings;
        }

        protected override bool HasLock => _lock?.IsHeld ?? false;

        protected override void GetLockSync()
        {
            _lock = _client.AcquireLock(new LockOptions(_settings.Key)
            {
                SessionTTL = TimeSpan.FromSeconds(_settings.SessionTTL),
                LockWaitTime = TimeSpan.FromSeconds(3),
                LockTryOnce = true
            }).Result;
        }
    }
}
