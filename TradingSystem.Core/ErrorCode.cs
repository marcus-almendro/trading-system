using Newtonsoft.Json;

namespace TradingSystem.Core
{
    public partial class ErrorCode
    {
        public int Value { get; set; }

        [JsonIgnore]
        public string Description
        {
            get
            {
                switch (Value)
                {
                    case 1: return nameof(Success);
                    case 2: return nameof(AlreadyExists);
                    case 3: return nameof(InvalidSymbol);
                    case 4: return nameof(InvalidOrderId);
                    case 5: return nameof(InvalidArgument);
                    case 6: return nameof(Timeout);
                    default: return "Unknown Code";
                }
            }
        }

        public ErrorCode()
        {
            
        }
        
        public ErrorCode(int errorCode)
        {
            this.Value = errorCode;
        }

        public static ErrorCode Success = new ErrorCode(1);
        public static ErrorCode AlreadyExists = new ErrorCode(2);
        public static ErrorCode InvalidSymbol = new ErrorCode(3);
        public static ErrorCode InvalidOrderId = new ErrorCode(4);
        public static ErrorCode InvalidArgument = new ErrorCode(5);
        public static ErrorCode Timeout = new ErrorCode(6);
    }
}