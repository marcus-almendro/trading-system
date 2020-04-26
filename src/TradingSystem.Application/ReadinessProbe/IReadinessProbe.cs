namespace TradingSystem.Application.ReadinessProbe
{
    public interface IReadinessProbe
    {
        bool IsReady { get; }
    }
}