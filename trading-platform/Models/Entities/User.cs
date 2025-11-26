namespace trading_platform.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Wallet? Wallet { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
