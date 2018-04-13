namespace TradingSystem.Core
{
    public class Trade
    {
        public Trade() { }
        
        public Trade(long id, long takenOrderId, long takerOrderId, Side takerSide, long price, long executedSize, long remainingSize, int takenUserId, int takerUserId)
        {
            Id = id;
            TakenOrderId = takenOrderId;
            TakerOrderId = takerOrderId;
            TakerSide = takerSide;
            Price = price;
            ExecutedSize = executedSize;
            RemainingSize = remainingSize;
            TakenUserId = takenUserId;
            TakerUserId = takerUserId;
        }

        public long Id { get; set; }
        public long TakenOrderId { get; set; }
        public long TakerOrderId { get; set; }
        public Side TakerSide { get; set; }
        public long Price { get; set; }
        public long ExecutedSize { get; set; }
        public long RemainingSize { get; set; }
        public int TakenUserId { get; set; }
        public int TakerUserId { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Trade;

            if (other == null
                || Id != other.Id
                || TakenOrderId != other.TakenOrderId
                || TakerOrderId != other.TakerOrderId
                || TakerSide != other.TakerSide
                || Price != other.Price
                || ExecutedSize != other.ExecutedSize
                || RemainingSize != other.RemainingSize
                || TakenUserId != other.TakenUserId
                || TakerUserId != other.TakerUserId)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode()
                   ^ TakenOrderId.GetHashCode()
                   ^ TakerOrderId.GetHashCode()
                   ^ TakerSide.GetHashCode()
                   ^ Price.GetHashCode()
                   ^ ExecutedSize.GetHashCode()
                   ^ RemainingSize.GetHashCode()
                   ^ TakenUserId.GetHashCode()
                   ^ TakerUserId.GetHashCode();
        }
    }
}