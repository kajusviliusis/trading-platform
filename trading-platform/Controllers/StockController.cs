using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using trading_platform.Data;
using trading_platform.Models.Entities;

namespace trading_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly IConfiguration _config;
        public StockController(TradingDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
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
        [HttpGet("live/{symbol}")]
        public async Task<IActionResult> GetLivePrice(string symbol)
        {
            try
            {
                using var client = new HttpClient();
                var apiKey = _config["Finnhub:ApiKey"];
                client.DefaultRequestHeaders.Add("X-Finnhub-Token", apiKey);

                var url = $"https://finnhub.io/api/v1/quote?symbol={symbol}";
                var response = await client.GetStringAsync(url);

                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;
                var price = root.GetProperty("c").GetDecimal();

                return Ok(new { Symbol = symbol, Price = price });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
