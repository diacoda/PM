using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> b)
    {
        // Primary key
        b.HasKey(t => t.Id);

        // Basic properties
        b.Property(t => t.Date)
            .IsRequired();

        b.Property(t => t.Type)
            .HasConversion<int>()
            .IsRequired();

        b.Property(t => t.Quantity)
            .IsRequired();

        b.OwnsOne(t => t.Instrument, i =>
        {
            i.Property(ins => ins.Name)
                .HasMaxLength(100);

            i.Property(ins => ins.AssetClass)
                .HasConversion<int>();

            i.OwnsOne(ins => ins.Symbol, s =>
            {
                s.Property(sym => sym.Value)
                    .HasMaxLength(20)
                    .IsRequired();
            });
        });

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
