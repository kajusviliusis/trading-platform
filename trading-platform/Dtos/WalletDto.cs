namespace trading_platform.Dtos
{
    public class CreateWalletDto
    {
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public int UserId { get; set; }
    }
    public class UpdateWalletDto
    {
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
    public class WalletResponseDto
    {
        public int Id { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public int UserId { get; set; }

        public decimal PortfolioValue { get; set; }
        public decimal TotalBalance { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
