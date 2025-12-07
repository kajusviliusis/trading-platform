namespace trading_platform.Dtos
{
    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public int StockId { get; set; }
        public decimal Quantity { get; set; }
        public string Type { get; set; } = string.Empty;
    }
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StockId { get; set; }
        public decimal Quantity { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public decimal PriceAtExecution { get; set; }
    }
}
