using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Values;

namespace PM.Infrastructure.Data.Configurations;

public class FxRateConfiguration : IEntityTypeConfiguration<FxRate>
{
    public void Configure(EntityTypeBuilder<FxRate> b)
    {
        var currencyConverter = ValueConverters.CurrencyConverter;

        b.HasKey(f => new { f.FromCurrency, f.ToCurrency, f.Date });

        b.Property(f => f.FromCurrency)
            .HasConversion(currencyConverter)
            .HasMaxLength(3)
            .IsRequired();

        b.Property(f => f.ToCurrency)
            .HasConversion(currencyConverter)
            .HasMaxLength(3)
            .IsRequired();

        b.Property(f => f.Date)
            .IsRequired();

        b.Property(f => f.Rate)
            .HasPrecision(18, 6)
            .IsRequired();

        b.HasIndex(f => new { f.FromCurrency, f.ToCurrency });
    }
}
