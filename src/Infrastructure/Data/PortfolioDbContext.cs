using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data
{
    /// <summary>
    /// Database context for portfolio structure and transaction data.
    /// </summary>
    public class PortfolioDbContext : DbContext
    {
        public PortfolioDbContext(DbContextOptions<PortfolioDbContext> options)
            : base(options) { }

        public DbSet<Portfolio> Portfolios => Set<Portfolio>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Holding> Holdings => Set<Holding>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Tag> Tags => Set<Tag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply entity configurations (if you have separate files)
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);

            // --- Example explicit mappings (safe defaults if you skip configuration files) ---

            // Portfolio
            modelBuilder.Entity<Portfolio>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Owner).IsRequired();
                b.HasMany(p => p.Accounts)
                    .WithOne(a => a.Portfolio)
                    .HasForeignKey(a => a.PortfolioId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Account
            modelBuilder.Entity<Account>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.Name).IsRequired();
                b.Property(a => a.CurrencyCode).IsRequired();
                b.HasMany(a => a.Holdings)
                    .WithOne(h => h.Account)
                    .HasForeignKey(h => h.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasMany(a => a.Transactions)
                    .WithOne(t => t.Account)
                    .HasForeignKey(t => t.AccountId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Holding
            modelBuilder.Entity<Holding>(b =>
            {
                b.HasKey(h => h.Id);
                b.Property(h => h.Quantity).IsRequired();

                // Symbol as owned type
                /*b.OwnsOne(h => h.Asset, sb =>
                {
                    sb.Property(s => s.Code).HasColumnName("SymbolCode").IsRequired();
                    sb.Property(s => s.AssetClass).HasColumnName("AssetClass").IsRequired();
                    sb.OwnsOne(s => s.Currency, cb =>
                    {
                        cb.Property(c => c.Code).HasColumnName("Currency").IsRequired();
                    });
                });*/

                // EF owns concrete Asset, domain exposes IAsset
                b.OwnsOne<Asset>("_asset", sb =>
                {
                    sb.Property(a => a.Code).HasColumnName("AssetCode").IsRequired();
                    sb.Property(a => a.AssetClass).HasColumnName("AssetClass").IsRequired();
                    sb.OwnsOne(a => a.Currency, cb =>
                    {
                        cb.Property(c => c.Code).HasColumnName("AssetCurrency").IsRequired();
                    });
                });
                // tell EF to use backing field for Asset
                b.Ignore(h => h.Asset); // domain property is IAsset
            });

            // Transaction
            modelBuilder.Entity<Transaction>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Date).IsRequired();
                b.Property(t => t.Type).IsRequired();
                b.Property(t => t.Quantity).IsRequired();

                // Symbol as owned type
                b.OwnsOne(t => t.Symbol, sb =>
                {
                    sb.Property(s => s.Code).HasColumnName("AssetCode").IsRequired();
                    sb.Property(s => s.AssetClass).HasColumnName("AssetClass").IsRequired();
                    sb.OwnsOne(s => s.Currency, cb =>
                    {
                        cb.Property(c => c.Code).HasColumnName("Currency").IsRequired();
                    });
                });

                // Money as owned type
                b.OwnsOne(t => t.Amount, mb =>
                {
                    mb.Property(m => m.Amount).HasColumnName("Amount").IsRequired();
                    mb.OwnsOne(m => m.Currency, cb =>
                    {
                        cb.Property(c => c.Code).HasColumnName("AmountCurrency").IsRequired();
                    });
                });

                // Costs as optional owned type
                b.OwnsOne(t => t.Costs, cb =>
                {
                    cb.Property(m => m.Amount).HasColumnName("CostAmount");
                    cb.OwnsOne(m => m.Currency, ccb =>
                    {
                        ccb.Property(c => c.Code).HasColumnName("CostCurrency");
                    });
                });
            });

            // Tag (simple)
            modelBuilder.Entity<Tag>(b =>
            {
                b.HasKey(t => t.Id);
                b.Property(t => t.Name).IsRequired();
            });
        }
    }
}
