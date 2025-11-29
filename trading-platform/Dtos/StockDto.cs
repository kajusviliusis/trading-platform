namespace trading_platform.Dtos
{
    public class CreateStockDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string Name {  get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
    public class UpdateStockDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
    public class StockResponseDto
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
