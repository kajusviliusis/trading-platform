namespace trading_platform.Models.Entities
{
    public class Holding
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StockId { get; set; }
        public int Quantity { get; set; }
    }
}
