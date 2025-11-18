using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Values;

namespace PM.Infrastructure.Data.Configurations;

public class ValuationSnapshotConfiguration : IEntityTypeConfiguration<ValuationSnapshot>
{
    public void Configure(EntityTypeBuilder<ValuationSnapshot> builder)
    {
        builder.ToTable("ValuationRecords");

        builder.HasKey(v => v.Id);

        // Enums
        builder.Property(v => v.Kind)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(v => v.Period)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(v => v.AssetClass)
               .HasConversion<int?>();

        var currencyConverter = ValueConverters.CurrencyConverter;

        builder.Property(v => v.ReportingCurrency)
               .HasConversion(currencyConverter)
               .HasMaxLength(3)
               .IsRequired();

        // Scalar fields
        builder.Property(v => v.Owner)
               .HasMaxLength(200);

        builder.Property(v => v.Type)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(v => v.AccountId);
        builder.Property(v => v.PortfolioId);

        // Percentages
        builder.Property(v => v.Percentage)
               .HasPrecision(9, 6);

        // Inline Money mapping — no FK / key generated
        MapMoney(builder, v => v.Value, "Value_Amount", "Value_Currency");
        MapMoney(builder, v => v.SecuritiesValue, "SecuritiesValue_Amount", "SecuritiesValue_Currency");
        MapMoney(builder, v => v.CashValue, "CashValue_Amount", "CashValue_Currency");
        MapMoney(builder, v => v.IncomeForDay, "IncomeForDay_Amount", "IncomeForDay_Currency");
    }

    private void MapMoney(
        EntityTypeBuilder<ValuationSnapshot> builder,
        Expression<Func<ValuationSnapshot, Money?>> propertyExpression,
        string amountColumn,
        string currencyColumn)
    {
        builder.OwnsOne(propertyExpression, mb =>
        {
            mb.Property(m => m.Amount)
              .HasColumnName(amountColumn)
              .HasPrecision(18, 4)
              .IsRequired(); // allow nulls since Money? is nullable

            mb.Property(m => m.Currency)
              .HasColumnName(currencyColumn)
              .HasMaxLength(3)
              .HasConversion(ValueConverters.CurrencyConverter)
              .IsRequired();

            mb.WithOwner(); // inline mapping, no FK or separate key
        });
    }

}
