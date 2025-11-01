using System.Text.Json;

namespace PM.Infrastructure.Providers;

public abstract class YahooBaseProvider
{
    private readonly IHttpClientFactory _httpClientFactory;

    protected YahooBaseProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    protected async Task<YahooResponse?> FetchYahooChartAsync(string ticker, DateOnly date, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        string range = PickRange(date);
        string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{Uri.EscapeDataString(ticker)}?interval=1d&range={range}";

        var resp = await client.GetAsync(url, ct);
        if (!resp.IsSuccessStatusCode)
            return null;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            return await JsonSerializer.DeserializeAsync<YahooResponse>(stream, options);
        }
        catch
        {
            return null;
        }
    }

    protected static decimal? ExtractCloseForDate(YahooResponse? response, DateOnly targetDate)
    {
        if (response?.chart?.result == null || response.chart.result.Length == 0)
            return null;

        var result = response.chart.result[0];
        var timestamps = result.timestamp;
        var closes = result.indicators?.quote?.FirstOrDefault()?.close;

        if (timestamps == null || closes == null || closes.Length != timestamps.Length)
            return null;

        for (int i = 0; i < timestamps.Length; i++)
        {
            var dtUtc = DateTimeOffset.FromUnixTimeSeconds(timestamps[i]).UtcDateTime;
            var entryDate = DateOnly.FromDateTime(dtUtc);

            if (entryDate == targetDate)
            {
                var close = closes[i];
                if (!double.IsNaN(close))
                    return Convert.ToDecimal(close);
            }
        }

        return null;
    }

    protected static string PickRange(DateOnly targetDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int days = today.DayNumber - targetDate.DayNumber;

        if (days <= 0) return "1d";
        if (days <= 5) return "5d";
        if (days <= 30) return "1mo";
        if (days <= 90) return "3mo";
        if (days <= 180) return "6mo";
        if (days <= 365) return "1y";
        return "max";
    }

    // JSON Schema shared by Yahoo Finance Chart API
    public class YahooResponse
    {
        public Chart? chart { get; set; }

        public class Chart
        {
            public Result[]? result { get; set; }
        }

        public class Result
        {
            public long[]? timestamp { get; set; }
            public Indicator? indicators { get; set; }
        }

        public class Indicator
        {
            public Quote[]? quote { get; set; }
        }

        public class Quote
        {
            public double[]? close { get; set; }
        }
    }
}
