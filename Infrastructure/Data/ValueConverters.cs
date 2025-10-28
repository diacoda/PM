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
        var parts = dbValue.Split('|');
        if (parts.Length != 2)
            throw new InvalidOperationException($"Invalid symbol format in DB: {dbValue}");
        return new Symbol(parts[0], parts[1]);
    }
    // Symbol converter: stores as "VALUE|CURRENCY"
    public static ValueConverter<Symbol, string> SymbolStringConverter =
        new ValueConverter<Symbol, string>(
            v => $"{v.Value}|{v.Currency}",         // to db
            v => CreateSymbolFromDbString(v)        // from db
        );
}
