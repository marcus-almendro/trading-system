using TradingSystem.Domain.Common;

namespace TradingSystem.Application.DTO
{
    public class ErrorCodeDTO
    {
        public ErrorCodeDTO()
        {

        }

        public ErrorCodeDTO(ErrorCode errorCode)
        {
            Value = errorCode.Value;
            Description = errorCode.Description;
        }

        public int Value { get; set; }
        public string Description { get; set; }

        public override string ToString() => $"{{Value: {Value} Description: {Description}}}";
    }
}
