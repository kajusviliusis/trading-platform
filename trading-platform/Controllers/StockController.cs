using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Text.Json;
using trading_platform.Data;
using trading_platform.Dtos;
using trading_platform.Models.Entities;
using trading_platform.Services;

namespace trading_platform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly TradingDbContext _context;
        private readonly IConfiguration _config;
        private readonly FinnhubService _finnhub;
        public StockController(TradingDbContext context, IConfiguration config, FinnhubService finnhub)
        {
            _context = context;
            _config = config;
            _finnhub = finnhub;
        }
        [HttpGet]
        public async Task<IActionResult> GetStocks()
        {
            var symbols = _context.Stocks.Select(s => s.Symbol).ToList();
            var quotes = new List<StockQuoteDto>();

            foreach (var symbol in symbols)
            {
                var dto = await _finnhub.GetQuoteAsync(symbol);
                if (dto != null) quotes.Add(dto);
            }

            return Ok(quotes);
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
            var dto = await _finnhub.GetQuoteAsync(symbol);
            if (dto == null) return NotFound();

            return Ok(dto);
        }
    }
}
