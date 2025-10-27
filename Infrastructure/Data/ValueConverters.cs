using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PM.Domain.Values;

public static class ValueConverters
{
    // Convert Currency to string
    public static ValueConverter<Currency, string> CurrencyConverter =
        new ValueConverter<Currency, string>(
            c => c.Code,
            s => new Currency(s));

    // Convert Money to decimal (store Amount) and currency separately
    public static ValueConverter<Money, string> MoneyCurrencyConverter =
        new ValueConverter<Money, string>(
            m => m.Currency.Code,
            s => new Money(0, new Currency(s)));

    public static ValueConverter<Money, decimal> MoneyAmountConverter =
        new ValueConverter<Money, decimal>(
            m => m.Amount,
            d => new Money(d, Currency.CAD)); // default, will override currency with MoneyCurrencyConverter
}
