namespace TradingSystem.Domain.Common
{
    public class ErrorCode
    {
        public ErrorCode(int errorCode)
        {
            Value = errorCode;
        }

        public int Value { get; }

        public string Description
        {
            get
            {
                return Value switch
                {
                    1 => nameof(Success),
                    2 => nameof(AlreadyExists),
                    3 => nameof(InvalidSymbol),
                    4 => nameof(InvalidOrderId),
                    5 => nameof(InvalidArgument),
                    6 => nameof(Timeout),
                    7 => nameof(OperationDeniedNodeIsNotLeader),
                    8 => nameof(OperationDeniedNodeIsNotFollower),
                    9 => nameof(Unauthorized),
                    _ => "Unknown Code",
                };
            }
        }

        public static ErrorCode Success = new ErrorCode(1);
        public static ErrorCode AlreadyExists = new ErrorCode(2);
        public static ErrorCode InvalidSymbol = new ErrorCode(3);
        public static ErrorCode InvalidOrderId = new ErrorCode(4);
        public static ErrorCode InvalidArgument = new ErrorCode(5);
        public static ErrorCode Timeout = new ErrorCode(6);
        public static ErrorCode OperationDeniedNodeIsNotLeader = new ErrorCode(7);
        public static ErrorCode OperationDeniedNodeIsNotFollower = new ErrorCode(8);
        public static ErrorCode Unauthorized = new ErrorCode(9);
    }
}