using Microsoft.AspNetCore.Mvc;
using trading_platform.Data;
using trading_platform.Models.Entities;

namespace trading_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly TradingDbContext _context;
        public StockController(TradingDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetStocks()
        {
            var stocks = _context.Stocks.ToList();
            return Ok(stocks);
        }
        [HttpGet("{id}")]
        public IActionResult GetStock(int id)
        {
            var stock = _context.Stocks.Find(id);
            if (stock == null)
            {
                return NotFound();
            }
            return Ok(stock);
        }
        [HttpPost]
        public IActionResult CreateStock(Stock stock)
        {
            _context.Stocks.Add(stock);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetStock), new { id = stock.Id}, stock );
        }
        [HttpPut("{id}")]
        public IActionResult UpdateStock(int id, Stock updated)
        {
            var stock = _context.Stocks.Find(id);
            if (stock == null)
            {
                return NotFound();
            }
            stock.Symbol = updated.Symbol;
            stock.Name = updated.Name;
            stock.Price = updated.Price;
            stock.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();
            return Ok(stock);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteStock(int id)
        {
            var stock = _context.Stocks.Find(id);
            if (stock == null)
            {
                return NotFound();
            }
            _context.Stocks.Remove(stock);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
