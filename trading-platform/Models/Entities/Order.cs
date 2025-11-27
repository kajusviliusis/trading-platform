namespace trading_platform.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public string Type { get; set; } = "Buy";
        public decimal PriceAtExcecution { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int UserId { get; set; }
        public User? User { get; set; }
        public int StockId { get; set; }
        public Stock? Stock { get; set; }
    }
}
