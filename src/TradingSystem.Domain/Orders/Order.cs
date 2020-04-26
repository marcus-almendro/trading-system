using TradingSystem.Domain.Common;
using TradingSystem.Domain.Events.Orders;

namespace TradingSystem.Domain.Orders
{
    public class Order : Entity
    {
        public Order(string symbol, long id, OrderType orderType, long price, long size, int traderId)
        {
            Symbol = symbol;
            Id = id;
            OrderType = orderType;
            Price = price;
            Size = size;
            TraderId = traderId;
        }

        public string Symbol { get; }
        public long Id { get; }
        public OrderType OrderType { get; }
        public long Price { get; }
        public long Size { get; private set; }
        public int TraderId { get; }
        public bool IsDeleted => Size == 0;

        internal ErrorCode Update(long size)
        {
            if (size >= Size || size < 0)
                return ErrorCode.InvalidArgument;

            Size = size;

            if (Size == 0)
                InternalEvents.Add(new OrderDeleted(this));
            else
                InternalEvents.Add(new OrderUpdated(this));

            return ErrorCode.Success;
        }

        internal ErrorCode Update(long size, int traderId)
        {
            if (TraderId != traderId)
                return ErrorCode.Unauthorized;

            return Update(size);
        }

        internal ErrorCode Delete() => Update(0);

        internal ErrorCode Delete(int traderId)
        {
            if (TraderId != traderId)
                return ErrorCode.Unauthorized;

            return Delete();
        }

        public override string ToString() => $"{{Type:{GetType().Name} Id: {Id} Price: {Price} Size: {Size} TraderId: {TraderId}}}";
    }
}