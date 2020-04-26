namespace TradingSystem.Application.DTO.Commands
{
    public class DeleteOrderCommand
    {
        public string Symbol { get; set; }
        public long Id { get; set; }
        public int TraderId { get; set; }

        public override string ToString() => $"{{Symbol: {Symbol} Id: {Id} TraderId: {TraderId}}}";
    }
}
