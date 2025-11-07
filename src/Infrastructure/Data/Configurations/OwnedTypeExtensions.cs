using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Values;

namespace PM.Infrastructure.Data.Configurations;

public static class OwnedTypeExtensions
{


    public static void ConfigureMoney<T>(this OwnedNavigationBuilder<T, Money> builder, string? amountColumn = null, string? currencyColumn = null)
        where T : class
    {
        var currencyConverter = ValueConverters.CurrencyConverter;

        builder.Property(m => m.Amount)
               .HasColumnName(amountColumn ?? "Amount")
               .HasPrecision(18, 4)
               .IsRequired();

        builder.Property(m => m.Currency)
               .HasColumnName(currencyColumn ?? "Currency")
               .HasMaxLength(3)
               .HasConversion(currencyConverter)
               .IsRequired();
    }
}
