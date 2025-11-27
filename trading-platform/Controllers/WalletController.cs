using Microsoft.AspNetCore.Mvc;
using trading_platform.Data;
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
            var wallets = _context.Wallets.ToList();
            return Ok(wallets);
        }
        [HttpGet("{id}")]
        public IActionResult GetWallet(int id)
        {
            var wallet = _context.Wallets.Find(id);
            if (wallet == null)
            {
                return NotFound();
            }
            return Ok(wallet);
        }
        [HttpPost]
        public IActionResult CreateWallet(Wallet wallet)
        {
            _context.Wallets.Add(wallet);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetWallet), new { id=wallet.Id}, wallet);
        }
        [HttpPut("{id}")]
        public IActionResult UpdateWallet(int id, Wallet updated)
        {
            var wallet = _context.Wallets.Find(id);
            if (wallet == null)
            {
                return NotFound();
            }

            wallet.Balance = updated.Balance;
            wallet.Currency = updated.Currency;
            wallet.UserId = updated.UserId;

            _context.SaveChanges();
            return Ok(wallet);
        }
        [HttpDelete]
        public IActionResult DeleteWallet(int id)
        {
            var wallet = _context.Wallets.Find(id);
            if (wallet == null)
            {
                return NotFound();
            }
            _context.Wallets.Remove(wallet);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
