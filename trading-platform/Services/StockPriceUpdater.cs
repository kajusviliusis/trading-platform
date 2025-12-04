using trading_platform.Data;

namespace trading_platform.Services
{
    public class StockPriceUpdater : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly FinnhubService _finnhub;

        public StockPriceUpdater(IServiceScopeFactory scopeFactory, FinnhubService finnhub)
        {
            _scopeFactory = scopeFactory;
            _finnhub = finnhub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TradingDbContext>();

                var stocks = db.Stocks.ToList();
                foreach (var stock in stocks)
                {
                    var quote = await _finnhub.GetQuoteAsync(stock.Symbol);
                    if (quote != null)
                    {
                        stock.Price = quote.CurrentPrice;
                        stock.UpdatedAt = DateTime.UtcNow;
                    }
                }
                await db.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }

}
