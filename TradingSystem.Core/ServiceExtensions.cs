namespace TradingSystem.Core
{
    public static class ServiceExtensions
    {
        public static OrderMsg ToOrderMsg(this Order order)
        {
            return new OrderMsg {
                Action = (int)order.Action,
                Id = order.Id,
                IsBuySide = order.Side == Side.Buy,
                Price = order.Price,
                Size = order.Size,
                UserId = order.UserId
            };
        }

        public static Order ToOrder(this OrderMsg order)
        {
            return new Order(
                (OrderEntryType)order.Action,
                order.Id,
                order.IsBuySide ? Side.Buy : Side.Sell,
                order.Price,
                order.Size,
                order.UserId,
                order.Position);
        }

        public static ErrorCodeMsg ToErrorCodeMsg(this ErrorCode errorCode)
        {
            return new ErrorCodeMsg { Value = errorCode.Value };
        }

        public static ErrorCode ToErrorCode(this ErrorCodeMsg errorCode)
        {
            return new ErrorCode(errorCode.Value);
        }
    }
}