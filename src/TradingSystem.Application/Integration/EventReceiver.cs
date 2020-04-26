using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TradingSystem.Application.Integration.Ports;
using TradingSystem.Application.Lifecycle;

namespace TradingSystem.Application.Integration
{
    public abstract class EventReceiver<TSource, TDestination> : IEventReceiver<TDestination>
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger<EventReceiver<TSource, TDestination>> _logger;
        private readonly ILifecycleManager _lifecycleManager;

        public event Action<TDestination> OnEvent;

        public EventReceiver(ILifecycleManager lifecycleManager, ILogger<EventReceiver<TSource, TDestination>> logger)
        {
            SetupTask();
            _lifecycleManager = lifecycleManager;
            _logger = logger;
            _lifecycleManager.StatusChanged += s =>
            {
                if (s == Status.BecomingLeader)
                    BecomeLeader();

                if (s == Status.StartingAsFollower)
                    BecomeFollower();
            };
        }

        public int TotalMessages { get; private set; }

        public CancellationToken StoppingToken { private get; set; }

        public void ReceiveUntil(long maxOffset)
        {
            _logger.LogInformation($"Consuming until {maxOffset}");
            BeginFollowing();
            _task.Start();
            WaitConsumptionEnd(maxOffset);
            Stop();
        }

        protected bool IsRunning => !StoppingToken.IsCancellationRequested && !_task.IsCompleted;

        protected virtual void BeginFollowing() => _logger.LogDebug("base.BeginFollowing called");

        protected abstract TSource ConsumeNextMessage();

        protected abstract TDestination Map(TSource obj);

        protected virtual void WaitConsumptionEnd(long maxOffset) => _logger.LogDebug("base.WaitConsumptionEnd called");

        protected virtual void BeginStopping() => _logger.LogDebug("base.BeginStopping called");

        private void BecomeLeader()
        {
            _logger.LogInformation("Waiting consumption end");
            WaitConsumptionEnd(-1);
            _logger.LogInformation("Consumption ended, waiting for task to finish...");
            Stop();
            _logger.LogInformation("Task completed");
            SetupTask();
        }

        private void BecomeFollower()
        {
            _logger.LogInformation("Becoming a follower, task started");
            BeginFollowing();
            _task.Start();
        }

        private void SetupTask()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _task = new Task(() => Loop(_cancellationTokenSource.Token));
        }

        private void Loop(CancellationToken taskCancellationToken)
        {
            try
            {
                while (!taskCancellationToken.IsCancellationRequested && !StoppingToken.IsCancellationRequested)
                {
                    var msg = ConsumeNextMessage();
                    if (msg != null)
                    {
                        _logger.LogDebug("Read message {msg}, dispatching...", msg);
                        var destinationMsg = Map(msg);
                        OnEvent?.Invoke(destinationMsg);
                        TotalMessages++;
                        _logger.LogDebug("Total messages dispatched so far: {totalMsg}", TotalMessages);
                    }
                    else
                        Task.Delay(100).Wait();
                }
            }
            catch (Exception e)
            {
                ThreadPool.QueueUserWorkItem(_ => { throw new Exception("Exception in EventReceiver Loop!", e); });
            }
        }

        private void Stop()
        {
            BeginStopping();
            _cancellationTokenSource.Cancel();
            _task.Wait();
            _cancellationTokenSource.Dispose();
            _task.Dispose();
        }

    }
}