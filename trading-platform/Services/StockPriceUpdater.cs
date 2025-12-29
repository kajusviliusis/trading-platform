using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using trading_platform.Data;
using trading_platform.Dtos;

namespace trading_platform.Services
{
    public class StockPriceUpdater : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FinnhubService _finnhub;
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;
        private readonly int _quoteTtlSeconds;

        public StockPriceUpdater(
            IServiceScopeFactory scopeFactory,
            FinnhubService finnhub,
            IConfiguration config,
            IDistributedCache cache)
        {
            _scopeFactory = scopeFactory;
            _finnhub = finnhub;
            _config = config;
            _cache = cache;
            _quoteTtlSeconds = Math.Max(1, _config.GetValue<int>("Finnhub:QuoteTtlSeconds", 30));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //limitai is configo
            var maxRps = Math.Max(1, _config.GetValue<int>("Finnhub:MaxRequestsPerSecond", 5));
            var interval = TimeSpan.FromSeconds(Math.Max(5, _config.GetValue<int>("Finnhub:UpdateIntervalSeconds", 150)));
            var perRequestDelay = TimeSpan.FromMilliseconds(1000.0 / maxRps);
            var batchSize = Math.Max(10, _config.GetValue<int>("Finnhub:BatchSize", 50));
            var minUpdateAge = TimeSpan.FromMinutes(_config.GetValue<int>("Finnhub:MinUpdateAgeMinutes", 5));

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();

                var cutoff = DateTime.UtcNow - minUpdateAge;

                // updatina seniausiai updatinta pirmiausia
                var stocks = db.Stocks
                    .Where(s => s.UpdatedAt <= cutoff)
                    .OrderBy(s => s.UpdatedAt)
                    .ThenBy(s => s.Symbol)
                    .Take(batchSize)
                    .ToList();

                if (stocks.Count == 0)
                {
                    stocks = db.Stocks
                        .OrderBy(s => s.UpdatedAt)
                        .ThenBy(s => s.Symbol)
                        .Take(batchSize)
                        .ToList();
                }

                var sawRateLimit = false;
                var updatedSymbols = new List<string>(capacity: stocks.Count);

                foreach (var stock in stocks)
                {
                    if (stoppingToken.IsCancellationRequested) break;
                    if (sawRateLimit) break;

                    try
                    {
                        var quote = await _finnhub.GetQuoteAsync(stock.Symbol);
                        if (quote != null)
                        {
                            stock.Price = quote.CurrentPrice;
                            stock.UpdatedAt = DateTime.UtcNow;
                            updatedSymbols.Add(stock.Symbol);
                        }
                    }
                    catch (HttpRequestException ex) when (ex.Message.Contains("429"))
                    {
                        // kai yra rate limitas sustabdom
                        sawRateLimit = true;
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Price update failed for {stock.Symbol}: {ex.Message}");
                    }

                    try
                    {
                        await Task.Delay(perRequestDelay, stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }

                try
                {
                    await db.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Saving updated prices failed: {ex.Message}");
                    updatedSymbols.Clear();
                }

                // redis cache updatinamas is postgres
                if (updatedSymbols.Count > 0)
                {
                    var options = new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_quoteTtlSeconds)
                    };

                    foreach (var symbol in updatedSymbols)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        var saved = stocks.FirstOrDefault(s => s.Symbol == symbol);
                        if (saved == null) continue;
                        var dto = new StockQuoteDto
                        {
                            Symbol = saved.Symbol,
                            CurrentPrice = saved.Price,
                            Open = saved.Price,
                            High = saved.Price,
                            Low = saved.Price,
                            PreviousClose = saved.Price
                        };

                        try
                        {
                            var json = JsonSerializer.Serialize(dto);
                            await _cache.SetStringAsync($"quote:{symbol}", json, options, stoppingToken).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to update cache for {symbol}: {ex.Message}");
                        }
                    }
                }

                // jei hitinam rate limita palaukiam updateintseconds ir + 30s
                var nextDelay = sawRateLimit ? interval + TimeSpan.FromSeconds(30) : interval;
                try
                {
                    await Task.Delay(nextDelay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
