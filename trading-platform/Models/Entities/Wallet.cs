namespace trading_platform.Models.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "USD";

        public int UserId { get; set; }
        public User User {  get; set; }
    }
}
