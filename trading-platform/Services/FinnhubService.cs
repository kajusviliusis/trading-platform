using Microsoft.AspNetCore.Http.HttpResults;
using System.Text.Json.Serialization;
using trading_platform.Dtos;

namespace trading_platform.Services
{
    public class FinnhubService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        public FinnhubService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Finnhub:ApiKey"];
        }

        public async Task<StockQuoteDto?> GetQuoteAsync(string symbol)
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
        }
        public async Task<IReadOnlyList<string>> GetSp500ConstituentsAsync()
        {
            var url = $"https://finnhub.io/api/v1/index/constituents?symbol=%5EGSPC&token={_apiKey}";
            var res = await _http.GetFromJsonAsync<FinnhubIndexConstituentsResponse>(url);
            return res?.Constituents ?? new List<string>();
        }
        public async Task<string> GetCompanyNameAsync(string symbol)
        {
            var url = $"https://finnhub.io/api/v1/stock/profile2?symbol={symbol}&token={_apiKey}";
            var res = await _http.GetFromJsonAsync<FinnhubProfile2Response>(url);
            return string.IsNullOrWhiteSpace(res?.Name) ? symbol : res!.Name!;
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
