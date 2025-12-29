using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using trading_platform.Dtos;

namespace trading_platform.Services
{
    public class FinnhubService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly IDistributedCache _cache;

        public FinnhubService(HttpClient http, IConfiguration config, IDistributedCache cache)
        {
            _http = http;
            _apiKey = config["Finnhub:ApiKey"];
            _cache = cache;
        }

        public Task<StockQuoteDto?> GetQuoteAsync(string symbol)
            => GetOrAddAsync<StockQuoteDto?>($"quote:{symbol}", async () =>
            {
                var url = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={_apiKey}";
                var res = await _http.GetFromJsonAsync<FinnhubQuoteResponse>(url);
                if (res == null) return null;
                return new StockQuoteDto
                {
                    Symbol = symbol,
                    CurrentPrice = res.CurrentPrice,
                    Open = res.Open,
                    High = res.High,
                    Low = res.Low,
                    PreviousClose = res.PreviousClose
                };
            }, TimeSpan.FromSeconds(30));

        public Task<IReadOnlyList<string>?> GetSp500ConstituentsAsync()
            => GetOrAddAsync<IReadOnlyList<string>?>("index:^GSPC", async () =>
            {
                var url = $"https://finnhub.io/api/v1/index/constituents?symbol=%5EGSPC&token={_apiKey}";
                var res = await _http.GetFromJsonAsync<FinnhubIndexConstituentsResponse>(url);
                return (IReadOnlyList<string>?)(res?.Constituents ?? new List<string>());
            }, TimeSpan.FromHours(24));

        public Task<string?> GetCompanyNameAsync(string symbol)
            => GetOrAddAsync<string?>($"company:{symbol}", async () =>
            {
                var url = $"https://finnhub.io/api/v1/stock/profile2?symbol={symbol}&token={_apiKey}";
                var res = await _http.GetFromJsonAsync<FinnhubProfile2Response>(url);
                return string.IsNullOrWhiteSpace(res?.Name) ? symbol : res!.Name!;
            }, TimeSpan.FromDays(30));

        // Simple cache-aside helper (no in-process coalescing)
        private async Task<T?> GetOrAddAsync<T>(string key, Func<Task<T?>> factory, TimeSpan ttl)
        {
            // Try distributed cache first
            var cached = await _cache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(cached))
            {
                try
                {
                    return JsonSerializer.Deserialize<T?>(cached);
                }
                catch
                {
                    // If cached payload is corrupt, remove it and fall back to factory
                    await _cache.RemoveAsync(key);
                }
            }

            // Cache miss - fetch from source
            var result = await factory().ConfigureAwait(false);

            if (result != null)
            {
                var json = JsonSerializer.Serialize(result);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                };
                await _cache.SetStringAsync(key, json, options).ConfigureAwait(false);
            }

            return result;
        }
    }

    public class FinnhubQuoteResponse
    {
        [JsonPropertyName("c")] public decimal CurrentPrice { get; set; }
        [JsonPropertyName("o")] public decimal Open { get; set; }
        [JsonPropertyName("h")] public decimal High { get; set; }
        [JsonPropertyName("l")] public decimal Low { get; set; }
        [JsonPropertyName("pc")] public decimal PreviousClose { get; set; }
    }
    public class FinnhubIndexConstituentsResponse
    {
        [JsonPropertyName("constituents")] public List<string> Constituents { get; set; } = new();
        [JsonPropertyName("symbol")] public string Symbol { get; set; } = string.Empty;
    }

    public class FinnhubProfile2Response
    {
        [JsonPropertyName("name")] public string? Name { get; set; }
        [JsonPropertyName("ticker")] public string? Ticker { get; set; }
        [JsonPropertyName("country")] public string? Country { get; set; }
        [JsonPropertyName("currency")] public string? Currency { get; set; }
        [JsonPropertyName("exchange")] public string? Exchange { get; set; }
    }
}
