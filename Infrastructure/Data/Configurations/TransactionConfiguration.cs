using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Configurations
{
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
             //.HasColumnType("decimal(18,6)")
             .IsRequired();

            // ----------------------------
            // Instrument as Owned Entity
            // ----------------------------
            b.OwnsOne(t => t.Instrument, i =>
            {
                i.Property(ins => ins.Name)
                 //.HasColumnName("InstrumentName")
                 .HasMaxLength(100);

                i.Property(ins => ins.AssetClass)
                 //.HasColumnName("InstrumentAssetClass")
                 .HasConversion<int>();

                i.OwnsOne(ins => ins.Symbol, s =>
                {
                    s.Property(sym => sym.Value)
                     //.HasColumnName("InstrumentSymbol")
                     .HasMaxLength(20)
                     .IsRequired();
                });
            });

            // ----------------------------
            // Amount as Owned Entity
            // ----------------------------
            b.OwnsOne(t => t.Amount, amount =>
            {
                // Amount value
                amount.Property(m => m.Amount)
                      //.HasColumnName("AmountValue")
                      //.HasColumnType("decimal(18,2)")
                      .IsRequired();

                // Amount currency
                amount.OwnsOne(m => m.Currency, c =>
                {
                    c.Property(cur => cur.Code)
                     //.HasColumnName("AmountCurrency")
                     .HasMaxLength(3)
                     .IsRequired();
                });
            });

            // ----------------------------
            // Costs as optional Owned Entity
            // ----------------------------
            b.OwnsOne(t => t.Costs, costs =>
            {
                costs.Property(m => m.Amount);
                //.HasColumnName("CostsValue")
                //.HasColumnType("decimal(18,2)");

                costs.OwnsOne(m => m.Currency, c =>
                {
                    c.Property(cur => cur.Code)
                     //.HasColumnName("CostsCurrency")
                     .HasMaxLength(3);
                });
            });

            // ----------------------------
            // Relationship to Account
            // ----------------------------
            b.HasOne(t => t.Account)
             .WithMany(a => a.Transactions)
             .HasForeignKey(t => t.AccountId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
