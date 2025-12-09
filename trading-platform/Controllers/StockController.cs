using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Text;
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
            var stocks = await _context.Stocks.ToListAsync();

            var response = stocks.Select(s => new StockResponseDto
            {
                Id = s.Id,
                Symbol = s.Symbol,
                Name = s.Name,
                Price = s.Price,
                UpdatedAt = s.UpdatedAt
            });

            return Ok(response);
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

            return CreatedAtAction(nameof(GetStock), new { id = stock.Id }, response);
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

        [HttpPost("seed/sp500")]
        public async Task<IActionResult> SeedSp500Async([FromQuery] bool fetchInitialPrices = false)
        {
            var symbols = await _finnhub.GetSp500ConstituentsAsync();
            if (symbols.Count == 0) return Problem("No constituents returned from Finnhub.");

            var existing = await _context.Stocks
                .Select(s => s.Symbol)
                .ToListAsync();

            var toInsert = symbols
                .Where(s => !existing.Contains(s))
                .ToList();

            if (toInsert.Count == 0)
            {
                return Ok(new { inserted = 0, message = "All S&P 500 symbols already exist." });
            }

            var now = DateTime.UtcNow;
            var batch = new List<Stock>(capacity: toInsert.Count);

            foreach (var symbol in toInsert)
            {
                var name = await _finnhub.GetCompanyNameAsync(symbol);
                decimal initialPrice = 0m;
                if (fetchInitialPrices)
                {
                    var quote = await _finnhub.GetQuoteAsync(symbol);
                    if (quote != null) initialPrice = quote.CurrentPrice;
                    //del limitu delay
                    await Task.Delay(50);
                }

                batch.Add(new Stock
                {
                    Symbol = symbol,
                    Name = name,
                    Price = initialPrice,
                    UpdatedAt = now
                });
            }

            await _context.Stocks.AddRangeAsync(batch);
            await _context.SaveChangesAsync();

            return Ok(new { inserted = batch.Count });
        }

        [HttpPost("seed/csv")]
        public async Task<IActionResult> SeedFromCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("CSV file is required.");

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

            var header = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(header)) return BadRequest("CSV header is missing.");

            var delimiter = header.Contains('\t') ? '\t' : ',';
            var headers = header.Split(delimiter, StringSplitOptions.TrimEntries);

            int idxSymbol = Array.FindIndex(headers, h => string.Equals(h, "Symbol", StringComparison.OrdinalIgnoreCase));
            int idxName = Array.FindIndex(headers, h =>
                string.Equals(h, "Security", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(h, "Name", StringComparison.OrdinalIgnoreCase));

            if (idxSymbol < 0) return BadRequest("CSV must contain a 'Symbol' header.");

            var existingSymbols = await _context.Stocks.Select(s => s.Symbol).ToListAsync();
            var now = DateTime.UtcNow;
            var batch = new List<Stock>();

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cells = line.Split(delimiter);
                if (idxSymbol >= cells.Length) continue;

                var symbol = cells[idxSymbol].Trim();
                if (string.IsNullOrWhiteSpace(symbol)) continue;
                if (existingSymbols.Contains(symbol)) continue;

                var name = (idxName >= 0 && idxName < cells.Length) ? cells[idxName].Trim() : symbol;

                batch.Add(new Stock
                {
                    Symbol = symbol,
                    Name = string.IsNullOrWhiteSpace(name) ? symbol : name,
                    Price = 0m,
                    UpdatedAt = now
                });
            }

            if (batch.Count == 0) return Ok(new { inserted = 0, message = "No new symbols to insert." });

            await _context.Stocks.AddRangeAsync(batch);
            await _context.SaveChangesAsync();

            return Ok(new { inserted = batch.Count });
        }
    }
}
