namespace TradingSystem.Application.Lifecycle
{
    public enum Status
    {
        Uninitialized,
        StartingAsFollower,
        RunningAsFollower,
        BecomingLeader,
        RunningAsLeader,
    }
}
