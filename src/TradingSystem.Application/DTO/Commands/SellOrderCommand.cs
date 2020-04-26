namespace TradingSystem.Application.DTO.Commands
{
    public class SellOrderCommand
    {
        public string Symbol { get; set; }
        public long Price { get; set; }
        public long Size { get; set; }
        public int TraderId { get; set; }

        public override string ToString() => $"{{Symbol: {Symbol} Price: {Price} Size: {Size} TraderId: {TraderId}}}";
    }
}
