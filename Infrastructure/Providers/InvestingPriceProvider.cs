using System.Collections.Generic;
using System.Threading.Tasks;
using PM.Domain.Entities;
using PM.Domain.Values;
using HtmlAgilityPack;
using System.Globalization;
using PM.Application.Interfaces;

namespace PM.Infrastructure.Providers;

public class InvestingPriceProvider : IPriceProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    public string ProviderName { get; } = "Investing";

    public InvestingPriceProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<InstrumentPrice?> GetPriceAsync(Symbol symbol, DateOnly date, CancellationToken ct = default)
    {
        string url = TdESeriesUrls[symbol.Code];
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException($"Symbol {symbol.Code} is not supported by {ProviderName}.");
        decimal value = await FetchTdESeriesPriceAsync(url, symbol.Code, ct);
        Currency currency = new Currency(symbol.Currency);
        return new InstrumentPrice(symbol, date, new Money(value, currency), currency, ProviderName);
    }

    private async Task<decimal> FetchTdESeriesPriceAsync(string url, string symbol, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

        string html = await client.GetStringAsync(url, ct);

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var priceNode = doc.DocumentNode.SelectSingleNode("//span[@id='last_last']");
        if (priceNode != null)
        {
            var priceText = priceNode.InnerText.Trim();
            if (decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                return price;
            }
            throw new ApplicationException($"Could not parse price for {symbol}.");
        }
        else
        {
            throw new ApplicationException($"Could not find price node for {symbol}.");
        }
    }
    private static readonly Dictionary<string, string> TdESeriesUrls = new()
    {
        { "TDB900", "https://ca.investing.com/funds/td-indiciel-canadien-e" },
        { "TDB902", "https://ca.investing.com/funds/td-us-index-e-cad" },
        { "TDB911", "https://ca.investing.com/funds/td-international-index-e" }
    };

}
