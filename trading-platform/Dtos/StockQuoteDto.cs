namespace trading_platform.Dtos
{
    public class StockQuoteDto
    {
        public string Symbol { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Open {  get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal PreviousClose { get; set; }
    }
}
