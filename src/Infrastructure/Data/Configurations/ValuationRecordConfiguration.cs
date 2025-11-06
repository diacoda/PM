using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Configurations;

public class ValuationRecordConfiguration : IEntityTypeConfiguration<ValuationRecord>
{
    public void Configure(EntityTypeBuilder<ValuationRecord> builder)
    {
        builder.HasKey(v => v.Id);

        var currencyConverter = ValueConverters.CurrencyConverter;

        builder.Property(v => v.ReportingCurrency)
               .HasConversion(currencyConverter)
               .HasMaxLength(3)
               .IsRequired();

        builder.OwnsOne(v => v.Value, mb => mb.ConfigureMoney("Value_Amount", "Value_Currency"));
        builder.OwnsOne(v => v.SecuritiesValue, mb => mb.ConfigureMoney("SecuritiesValue_Amount", "SecuritiesValue_Currency"));
        builder.OwnsOne(v => v.CashValue, mb => mb.ConfigureMoney("CashValue_Amount", "CashValue_Currency"));
        builder.OwnsOne(v => v.IncomeForDay, mb => mb.ConfigureMoney("IncomeForDay_Amount", "IncomeForDay_Currency"));

        builder.Property(v => v.Percentage).HasPrecision(5, 2);
        builder.Property(v => v.AssetClass).HasConversion<int>();
    }
}
