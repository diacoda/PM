using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Infrastructure.Data
{
    public class CashFlowDbContext : DbContext
    {
        public CashFlowDbContext(DbContextOptions<CashFlowDbContext> options)
            : base(options) { }

        public DbSet<CashFlow> CashFlows => Set<CashFlow>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations (if you have separate files)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CashFlowDbContext).Assembly);

            var currencyConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Currency, string>(
                c => c.Code,
                s => new Currency(s));

            modelBuilder.Entity<CashFlow>(b =>
            {
                b.HasKey(c => c.Id);

                b.Property(c => c.AccountId).IsRequired();

                // Map Money as owned type
                b.OwnsOne(c => c.Amount, m =>
                {
                    m.Property(x => x.Amount)
                        .HasColumnType("decimal(18,4)")
                        .IsRequired();

                    m.Property(x => x.Currency)
                        .HasConversion(currencyConverter)
                        .HasMaxLength(3)
                        .IsRequired();
                });

                b.Property(c => c.Type)
                    .HasConversion<int>()
                    .IsRequired();

                b.Property(c => c.Note)
                    .HasMaxLength(200);
            });
        }
    }
}
