namespace model.Domain.Values;
public record FxRate(Currency FromCurrency, Currency ToCurrency, DateTime Date, decimal Rate);