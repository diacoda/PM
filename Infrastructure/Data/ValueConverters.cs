using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PM.Domain.Values;

public static class ValueConverters
{
    // Convert Currency to string
    public static ValueConverter<Currency, string> CurrencyConverter =
        new ValueConverter<Currency, string>(
            c => c.Code,
            s => new Currency(s));

    //Money string
    public static ValueConverter<Money, string> MoneyCurrencyConverter =
        new ValueConverter<Money, string>(
            m => m.Currency.Code,
            s => new Money(0, new Currency(s)));
    //Money decimal
    public static ValueConverter<Money, decimal> MoneyAmountConverter =
        new ValueConverter<Money, decimal>(
            m => m.Amount,
            d => new Money(d, Currency.CAD)); // default, will override currency with MoneyCurrencyConverter

    private static Symbol CreateSymbolFromDbString(string dbValue)
    {
        return new Symbol(dbValue);
    }
    // Symbol converter: stores as "VALUE"
    public static ValueConverter<Symbol, string> SymbolStringConverter =
        new ValueConverter<Symbol, string>(
            m => m.Code,
            d => new Symbol(d));
}
