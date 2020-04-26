using Microsoft.Extensions.Logging;
using TradingSystem.Application.Integration.Ports;
using TradingSystem.Application.Lifecycle;

namespace TradingSystem.Application.Integration
{
    public abstract class EventDispatcher<TSource, TDestination> : IEventDispatcher<TSource>
    {
        private readonly ILifecycleManager _lifecycleManager;
        private readonly ILogger<EventDispatcher<TSource, TDestination>> _logger;

        public EventDispatcher(ILifecycleManager lifecycleManager, ILogger<EventDispatcher<TSource, TDestination>> logger)
        {
            _lifecycleManager = lifecycleManager;
            _logger = logger;
            _lifecycleManager.StatusChanged += s =>
            {
                if (s == Status.BecomingLeader)
                    BecomingLeader();

                if (s == Status.StartingAsFollower)
                    BecomingFollower();
            };
        }

        protected abstract TDestination Map(TSource obj);

        protected virtual void BecomingLeader() => _logger.LogDebug("base.OnBecomeLeader called");

        protected virtual void BecomingFollower() => _logger.LogDebug("base.OnBecomeFollower called");

        protected abstract void Publish(TDestination msg);

        public void Dispatch(TSource evt)
        {
            var msg = Map(evt);
            Publish(msg);
        }
    }
}