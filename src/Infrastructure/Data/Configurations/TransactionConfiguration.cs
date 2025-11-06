using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Date).IsRequired();
        builder.Property(t => t.Type).IsRequired();
        builder.Property(t => t.Quantity).IsRequired();

        // Symbol owned type
        builder.OwnsOne(t => t.Symbol, sb =>
        {
            sb.Property(s => s.Code).HasColumnName("AssetCode").IsRequired();
            sb.Property(s => s.AssetClass).HasColumnName("AssetClass").IsRequired();
            sb.OwnsOne(s => s.Currency, cb =>
            {
                cb.Property(c => c.Code).HasColumnName("Currency").IsRequired();
            });
        });

        // Money owned type
        builder.OwnsOne(t => t.Amount, mb => mb.ConfigureMoney("Amount", "AmountCurrency"));

        // Optional Costs
        builder.OwnsOne(t => t.Costs, mb => mb.ConfigureMoney("CostAmount", "CostCurrency"));
    }
}
