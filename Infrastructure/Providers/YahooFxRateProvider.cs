using PM.Application.Interfaces;
using PM.Domain.Values;
using PM.Infrastructure.Providers;

namespace PM.Infrastructure.Providers
{
    public class YahooFxProvider : YahooBaseProvider, IFxRateProvider
    {
        public string ProviderName => "Yahoo";

        public YahooFxProvider(IHttpClientFactory httpClientFactory)
            : base(httpClientFactory) { }

        public async Task<FxRate?> GetFxRateAsync(Currency fromCurrency, Currency toCurrency, DateOnly date, CancellationToken ct = default)
        {
            if (fromCurrency == null || toCurrency == null)
                throw new ArgumentNullException("Currencies must not be null.");

            string ticker = $"{fromCurrency.Code}{toCurrency.Code}=X";

            var response = await FetchYahooChartAsync(ticker, date, ct);
            var close = ExtractCloseForDate(response, date);

            if (close is null)
                return null;

            return new FxRate(fromCurrency, toCurrency, date, close.Value);
        }
    }
}
