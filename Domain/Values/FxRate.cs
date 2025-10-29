using System;
using PM.Domain.Values;

namespace PM.Domain.Values;

public record FxRate
{
    // EF Core needs parameterless constructor + setters
    private FxRate() { }

    public FxRate(Currency fromCurrency, Currency toCurrency, DateOnly date, decimal rate)
    {
        FromCurrency = fromCurrency ?? throw new ArgumentNullException(nameof(fromCurrency));
        ToCurrency = toCurrency ?? throw new ArgumentNullException(nameof(toCurrency));
        Date = date;
        Rate = rate;
    }

    public Currency FromCurrency { get; set; } = default!;
    public Currency ToCurrency { get; set; } = default!;
    public DateOnly Date { get; set; }
    public decimal Rate { get; set; }

    // Optional helper for queries
    public string Pair => $"{FromCurrency}/{ToCurrency}";
}
