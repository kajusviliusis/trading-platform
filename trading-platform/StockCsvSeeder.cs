using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text;
using trading_platform.Data;
using trading_platform.Models.Entities;

public static class StockCsvSeeder
{
    public static async Task SeedFromCsvAsync(TradingDbContext db, string csvPath)
    {
        if (await db.Stocks.AnyAsync()) return;

        if (!File.Exists(csvPath)) return;

        using var reader = new StreamReader(csvPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var header = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(header)) return;

        var delimiter = header.Contains('\t') ? '\t' : ',';
        var headers = header.Split(delimiter, StringSplitOptions.TrimEntries);

        int idxSymbol = Array.FindIndex(headers, h => string.Equals(h, "Symbol", StringComparison.OrdinalIgnoreCase));
        int idxName = Array.FindIndex(headers, h => string.Equals(h, "Security", StringComparison.OrdinalIgnoreCase) ||
                                                   string.Equals(h, "Name", StringComparison.OrdinalIgnoreCase));

        if (idxSymbol < 0) throw new Exception("CSV must contain a 'Symbol' column.");

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

            var name = (idxName >= 0 && idxName < cells.Length) ? cells[idxName].Trim() : symbol;

            batch.Add(new Stock
            {
                Symbol = symbol,
                Name = string.IsNullOrWhiteSpace(name) ? symbol : name,
                Price = 0m,
                UpdatedAt = now
            });
        }

        if (batch.Count > 0)
        {
            await db.Stocks.AddRangeAsync(batch);
            await db.SaveChangesAsync();
        }
    }
}
