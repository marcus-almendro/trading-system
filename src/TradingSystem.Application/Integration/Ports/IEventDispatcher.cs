namespace TradingSystem.Application.Integration.Ports
{
    public interface IEventDispatcher<T>
    {
        void Dispatch(T evt);
    }
}