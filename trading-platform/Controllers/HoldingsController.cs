using Microsoft.AspNetCore.Mvc;
using trading_platform.Data;

namespace trading_platform.Controllers
{
    [Controller]
    [Route("api/[controller]")]
    public class HoldingsController : ControllerBase
    {
        private readonly TradingDbContext _context;

        public HoldingsController(TradingDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetHoldingsByUser(int userId)
        {
            var holdings = _context.Holdings
                .Where(h => h.UserId == userId)
                .Select(h => new
                {
                    h.StockId,
                    h.Quantity,
                    StockName = h.Stock.Name,
                    CurrentPrice = h.Stock.Price
                })
                .ToList();

            return Ok(holdings);
        }
    }
}
