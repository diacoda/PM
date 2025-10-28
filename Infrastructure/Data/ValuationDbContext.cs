using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Infrastructure.Data
{
    public class ValuationDbContext : DbContext
    {
        public ValuationDbContext(DbContextOptions<ValuationDbContext> options)
            : base(options) { }

        public DbSet<ValuationRecord> ValuationRecords => Set<ValuationRecord>();
        public DbSet<InstrumentPrice> Prices => Set<InstrumentPrice>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var currencyConverter = ValueConverters.CurrencyConverter;
            var moneyAmountConverter = ValueConverters.MoneyAmountConverter;
            var symbolStringConverter = ValueConverters.SymbolStringConverter;

            // --- InstrumentPrice ---
            modelBuilder.Entity<InstrumentPrice>(builder =>
            {
                builder.HasKey(p => new { p.Symbol, p.Date });

                builder.Property(p => p.Symbol)
                    .HasConversion(symbolStringConverter)
                    .IsRequired()
                    .HasMaxLength(24);

                builder.Property(p => p.Date)
                    .IsRequired();

                builder.Property(p => p.Price)
                    .HasConversion(moneyAmountConverter)
                    .HasPrecision(18, 6) // safe for SQLite
                    .IsRequired();

                builder.Property(p => p.Currency)
                    .HasConversion(currencyConverter)
                    .HasMaxLength(3)
                    .IsRequired();

                builder.Property(p => p.Source)
                    .IsRequired();

                builder.Property(p => p.CreatedAtUtc)
                    .IsRequired();
            });

            // --- ValuationRecord ---
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
                        .HasPrecision(18, 4)
                        .IsRequired();
                    mv.Property(m => m.Currency)
                        .HasConversion(currencyConverter)
                        .HasMaxLength(3)
                        .IsRequired();
                });

                b.OwnsOne(v => v.SecuritiesValue, mv =>
                {
                    mv.Property(m => m.Amount).HasPrecision(18, 4);
                    mv.Property(m => m.Currency)
                        .HasConversion(currencyConverter)
                        .HasMaxLength(3);
                });

                b.OwnsOne(v => v.CashValue, mv =>
                {
                    mv.Property(m => m.Amount).HasPrecision(18, 4);
                    mv.Property(m => m.Currency)
                        .HasConversion(currencyConverter)
                        .HasMaxLength(3);
                });

                b.OwnsOne(v => v.IncomeForDay, mv =>
                {
                    mv.Property(m => m.Amount).HasPrecision(18, 4);
                    mv.Property(m => m.Currency)
                        .HasConversion(currencyConverter)
                        .HasMaxLength(3);
                });

                b.Property(v => v.Percentage)
                    .HasPrecision(5, 2);

                b.Property(v => v.AssetClass)
                    .HasConversion<int>();
            });
        }
    }
}
