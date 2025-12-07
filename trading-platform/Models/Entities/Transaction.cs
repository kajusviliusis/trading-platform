namespace trading_platform.Models.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StockId { get; set; }
        public int OrderId { get; set; }
        public decimal Quantity { get; set; }
        public decimal PriceAtExecution { get; set; }
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
    }
}
