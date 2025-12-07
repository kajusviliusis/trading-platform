using Microsoft.AspNetCore.Mvc;
using trading_platform.Data;
using trading_platform.Dtos;
using trading_platform.Models.Entities;

namespace trading_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletController : ControllerBase
    {
        private readonly TradingDbContext _context;
        public WalletController(TradingDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult GetWallets()
        {
            var wallets = _context.Wallets
                .Select(w => new WalletResponseDto
                {
                    Id = w.Id,
                    Balance = w.Balance,
                    Currency = w.Currency,
                    UserId = w.UserId
                })
                .ToList();
            return Ok(wallets);
        }
        [HttpGet("{id}")]
        public IActionResult GetWallet(int id)
        {
            var wallet = _context.Wallets.Find(id);
            if (wallet == null) return NotFound();
            var response = new WalletResponseDto
            {
                Id = wallet.Id,
                Balance = wallet.Balance,
                Currency = wallet.Currency,
                UserId = wallet.UserId
            };
            return Ok(response);
        }
        [HttpPost]
        public IActionResult CreateWallet(CreateWalletDto dto)
        {
            var wallet = new Wallet
            {
                Balance = dto.Balance,
                Currency = dto.Currency,
                UserId = dto.UserId
            };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();

            var response = new WalletResponseDto
            {
                Id = wallet.Id,
                Balance = wallet.Balance,
                Currency = wallet.Currency,
                UserId = wallet.UserId
            };

            return CreatedAtAction(nameof(GetWallet), new { id = wallet.Id }, response);
        }
        [HttpPut("{id}")]
        public IActionResult UpdateWallet(int id, UpdateWalletDto dto)
        {
            var wallet = _context.Wallets.Find(id);
            if (wallet == null) return NotFound();

            wallet.Balance = dto.Balance;
            wallet.Currency = dto.Currency;
            _context.SaveChanges();

            var response = new WalletResponseDto
            {
                Id = wallet.Id,
                Balance = wallet.Balance,
                Currency = wallet.Currency,
                UserId = wallet.UserId
            };

            return Ok(response);
        }
        [HttpDelete]
        public IActionResult DeleteWallet(int id)
        {
            var wallet = _context.Wallets.Find(id);
            if (wallet == null) return NotFound();
            _context.Wallets.Remove(wallet);
            _context.SaveChanges();
            return NoContent();
        }
        [HttpGet("user/{userId}/balance")]
        public async Task<IActionResult> GetUserBalance(int userId)
        {
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);
            var holdings = _context.Holdings.Where(h => h.UserId == userId).ToList();
            var stocks = _context.Stocks.ToList();

            decimal portfolioValue = 0;
            foreach (var h in holdings)
            {
                var stock = stocks.FirstOrDefault(s => s.Id == h.StockId);
                if (stock != null) portfolioValue += h.Quantity * stock.Price;
            }

            var totalBalance = (wallet?.Balance ?? 0) + portfolioValue;
            var dto = new WalletResponseDto
            {
                Balance = wallet?.Balance ?? 0,
                PortfolioValue = portfolioValue,
                TotalBalance = (wallet?.Balance ?? 0) + portfolioValue,
                UpdatedAt = DateTime.UtcNow
            };
            return Ok(dto);
        }
    }
}
