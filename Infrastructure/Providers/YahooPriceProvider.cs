namespace PM.Infrastructure.Providers;

using System.Text.Json;
using PM.Domain.Entities;
using PM.Application.Interfaces;
using PM.Domain.Values;
using PM.Infrastructure.Pricing.Yahoo;
public class YahooPriceProvider : IPriceProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    public string ProviderName => "Yahoo";

    public YahooPriceProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<InstrumentPrice?> GetPriceAsync(Symbol symbol, DateOnly date)
    {
        if (symbol is null) throw new ArgumentNullException(nameof(symbol));

        var ticker = Uri.EscapeDataString(symbol.Value);
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        string range = PickRange(date);

        string url = $"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}?interval=1d&range={range}";

        HttpResponseMessage resp = await client.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
            return null;

        try
        {
            using var stream = await resp.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = await JsonSerializer.DeserializeAsync<Response>(stream, options);
            if (response?.chart?.result is null || response.chart.result.Length == 0)
                return null;

            var result = response.chart.result[0];

            if (result.timestamp is null || result.indicators?.quote is null || result.indicators.quote.Length == 0)
                return null;

            var timestamps = result.timestamp;
            var closes = result.indicators.quote[0].close;

            if (closes == null || closes.Length != timestamps.Length)
                return null;

            for (int i = 0; i < timestamps.Length; i++)
            {
                var unix = timestamps[i];
                var dtUtc = DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
                var entryDate = DateOnly.FromDateTime(dtUtc.Date);

                if (entryDate == date)
                {
                    var close = closes[i];
                    if (double.IsNaN(close)) continue;
                    decimal amount = Convert.ToDecimal(close);
                    Currency currency = new Currency(symbol.Currency);
                    var price = new InstrumentPrice(symbol, date, new Money(amount, currency), currency, ProviderName);
                    return price;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private string PickRange(DateOnly targetDate)
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
}
