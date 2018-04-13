namespace TradingSystem.Core
{
    public class Order
    {
        public Order() { }
        
        public Order(OrderEntryType action, long id, Side side, long price, long size, int userId, int position)
        {
            Action = action;
            Id = id;
            Side = side;
            Price = price;
            Size = size;
            UserId = userId;
            Position = position;
        }

        public Order(Order copy, OrderEntryType action)
        {
            Action = action;
            Id = copy.Id;
            Side = copy.Side;
            Price = copy.Price;
            Size = copy.Size;
            UserId = copy.UserId;
            Position = copy.Position;
        }

        public OrderEntryType Action { get; set; }
        public long Id { get; set; }
        public Side Side { get; set; }
        public long Price { get; set; }
        public long Size { get; set; }
        public int UserId { get; set; }
        public int Position { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Order;

            if (other == null
                || Action != other.Action
                || Id != other.Id
                || Side != other.Side
                || Price != other.Price
                || Size != other.Size
                || UserId != other.UserId
                || Position != other.Position)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode()
                   ^ Id.GetHashCode()
                   ^ Side.GetHashCode()
                   ^ Price.GetHashCode()
                   ^ Size.GetHashCode()
                   ^ UserId.GetHashCode()
                   ^ Position.GetHashCode();
        }

        public Order Clone()
        {
            return (Order)MemberwiseClone();
        }
    }
}