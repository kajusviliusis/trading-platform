using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Text.Json;
using trading_platform.Data;
using trading_platform.Dtos;
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
            var stocks = _context.Stocks
                .Select(s => new StockResponseDto {
                    Id = s.Id,
                    Name = s.Name,
                    Symbol = s.Symbol,
                    Price = s.Price,
                    UpdatedAt = s.UpdatedAt

                    })
                .ToList();
            return Ok(stocks);
        }
        [HttpGet("{id}")]
        public IActionResult GetStock(int id)
        {
            var stock = _context.Stocks.Find(id);
            if (stock == null) return NotFound();

            var response = new StockResponseDto
            {
                Id = stock.Id,
                Name = stock.Name,
                Symbol = stock.Symbol,
                Price = stock.Price,
                UpdatedAt = stock.UpdatedAt
            };

            return Ok(response);
        }
        [HttpPost]
        public IActionResult CreateStock(CreateStockDto dto)
        {
            var stock = new Stock
            {
                Symbol = dto.Symbol,
                Price = dto.Price,
                Name = dto.Name,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Stocks.Add(stock);
            _context.SaveChanges();

            var response = new StockResponseDto
            {
                Id = stock.Id,
                Symbol = stock.Symbol,
                Name = stock.Name,
                Price = stock.Price,
                UpdatedAt = stock.UpdatedAt
            };

            return CreatedAtAction(nameof(GetStock), new { id = stock.Id}, response);
        }
        [HttpPut("{id}")]
        public IActionResult UpdateStock(int id, UpdateStockDto dto)
        {
            var stock = _context.Stocks.Find(id);
            if (stock == null) return NotFound();
            stock.Name = dto.Name;
            stock.Price = dto.Price;
            stock.UpdatedAt = DateTime.UtcNow;

            _context.SaveChanges();

            var response = new StockResponseDto
            {
                Id = stock.Id,
                Symbol = stock.Symbol,
                Name = stock.Name,
                Price = stock.Price,
                UpdatedAt = stock.UpdatedAt
            };

            return Ok(response);
        }
        [HttpDelete("{id}")]
        public IActionResult DeleteStock(int id)
        {
            var stock = _context.Stocks.Find(id);
            if (stock == null) return NotFound();
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

                var dto = new StockQuoteDto
                {
                    Symbol = symbol,
                    CurrentPrice = root.GetProperty("c").GetDecimal(),
                    Open = root.GetProperty("o").GetDecimal(),
                    High = root.GetProperty("h").GetDecimal(),
                    Low = root.GetProperty("l").GetDecimal(),
                    PreviousClose = root.GetProperty("pc").GetDecimal()
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
