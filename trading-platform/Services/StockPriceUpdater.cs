using trading_platform.Data;

namespace trading_platform.Services
{
    public class StockPriceUpdater : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FinnhubService _finnhub;
        private readonly IConfiguration _config;

        public StockPriceUpdater(IServiceScopeFactory scopeFactory, FinnhubService finnhub, IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _finnhub = finnhub;
            _config = config;
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

                await db.SaveChangesAsync(stoppingToken);

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
