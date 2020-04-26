namespace TradingSystem.Application.DTO.Commands
{
    public class UpdateOrderCommand
    {
        public string Symbol { get; set; }
        public long Id { get; set; }
        public long Size { get; set; }
        public int TraderId { get; set; }

        public override string ToString() => $"{{Symbol: {Symbol} Id: {Id} Size: {Size} TraderId: {TraderId}}}";
    }
}
