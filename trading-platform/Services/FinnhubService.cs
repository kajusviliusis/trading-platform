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

        public async Task<StockQuoteDto> GetQouteAsync(string symbol)
        {
            var url = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={_apiKey}";
            var res = await _http.GetFromJsonAsync<FinnhubQuoteResponse>(url);
            if(res == null) return null;
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
    }
    public class FinnhubQuoteResponse
    {
        [JsonPropertyName("c")] public decimal CurrentPrice { get; set; }
        [JsonPropertyName("o")] public decimal Open { get; set; }
        [JsonPropertyName("h")] public decimal High { get; set; }
        [JsonPropertyName("l")] public decimal Low { get; set; }
        [JsonPropertyName("pc")] public decimal PreviousClose { get; set; }
    }
}
