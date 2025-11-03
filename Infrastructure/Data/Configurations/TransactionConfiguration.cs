using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> b)
    {
        b.HasKey(t => t.Id);

        b.Property(t => t.Date)
            .IsRequired();

        b.Property(t => t.Type)
            .HasConversion<int>()
            .IsRequired();

        b.Property(t => t.Quantity)
            .IsRequired();

        // ✅ Owned type mapping for Symbol
        b.OwnsOne(t => t.Symbol, s =>
        {
            s.Property(x => x.Code)
                .HasMaxLength(20)
                .IsRequired();

            s.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            s.Property(x => x.Exchange)
                .HasMaxLength(10)
                .IsRequired();

            s.Property(x => x.AssetClass)
                .HasConversion<int>()
                .IsRequired();
        });

        // ✅ Owned type mapping for Amount
        b.OwnsOne(t => t.Amount, amount =>
        {
            amount.Property(m => m.Amount)
                .IsRequired();

            amount.OwnsOne(m => m.Currency, c =>
            {
                c.Property(cur => cur.Code)
                    .HasMaxLength(3)
                    .IsRequired();
            });
        });

        // ✅ Optional Costs
        b.OwnsOne(t => t.Costs, costs =>
        {
            costs.Property(m => m.Amount);

            costs.OwnsOne(m => m.Currency, c =>
            {
                c.Property(cur => cur.Code)
                    .HasMaxLength(3);
            });
        });

        b.HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}