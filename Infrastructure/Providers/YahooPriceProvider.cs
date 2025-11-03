using PM.Application.Interfaces;
using PM.Domain.Values;
using PM.Infrastructure.Providers;

namespace PM.Infrastructure.Providers
{
    public class YahooPriceProvider : YahooBaseProvider, IPriceProvider
    {
        public string ProviderName => "Yahoo";

        public YahooPriceProvider(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory) { }

        public async Task<InstrumentPrice?> GetPriceAsync(Symbol symbol, DateOnly date, CancellationToken ct = default)
        {
            if (symbol is null)
                throw new ArgumentNullException(nameof(symbol));

            var response = await FetchYahooChartAsync(symbol.Code, date, ct);
            var close = ExtractCloseForDate(response, date);
            if (close is null)
                return null;

            var currency = new Currency(symbol.Currency);
            return new InstrumentPrice(symbol, date, new Money(close.Value, currency), currency, ProviderName);
        }
    }
}
