using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PM.Infrastructure.Data
{
    public class ValuationDbContext : DbContext
    {
        public ValuationDbContext(DbContextOptions<ValuationDbContext> options)
            : base(options) { }

        public DbSet<ValuationRecord> ValuationRecords => Set<ValuationRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var currencyConverter = ValueConverters.CurrencyConverter;
            var moneyAmountConverter = ValueConverters.MoneyAmountConverter;

            modelBuilder.Entity<ValuationRecord>(b =>
            {
                b.HasKey(v => v.Id);

                b.Property(v => v.ReportingCurrency)
                    .HasConversion(currencyConverter)
                    .HasMaxLength(3)
                    .IsRequired();

                b.OwnsOne(v => v.Value, mv =>
                {
                    mv.Property(m => m.Amount)
                        .HasColumnType("decimal(18,4)")
                        .IsRequired();
                    mv.Property(m => m.Currency)
                        .HasConversion(currencyConverter)
                        .HasMaxLength(3)
                        .IsRequired();
                });

                b.OwnsOne(v => v.SecuritiesValue, mv =>
                {
                    mv.Property(m => m.Amount)
                        .HasColumnType("decimal(18,4)");
                    mv.Property(m => m.Currency)
                        .HasConversion(currencyConverter)
                        .HasMaxLength(3);
                });

                b.OwnsOne(v => v.CashValue, mv =>
                {
                    mv.Property(m => m.Amount)
                        .HasColumnType("decimal(18,4)");
                    mv.Property(m => m.Currency)
                        .HasConversion(currencyConverter)
                        .HasMaxLength(3);
                });

                b.OwnsOne(v => v.IncomeForDay, mv =>
                {
                    mv.Property(m => m.Amount)
                        .HasColumnType("decimal(18,4)");
                    mv.Property(m => m.Currency)
                        .HasConversion(currencyConverter)
                        .HasMaxLength(3);
                });

                b.Property(v => v.Percentage)
                    .HasColumnType("decimal(5,2)");

                b.Property(v => v.AssetClass)
                    .HasConversion<int>();
            });
        }
    }
}
