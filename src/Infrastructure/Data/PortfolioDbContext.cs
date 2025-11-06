using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Infrastructure.Data.Configuration;

namespace PM.Infrastructure.Data
{
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

            modelBuilder.ApplyConfiguration(new PortfolioConfiguration());
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new HoldingConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionConfiguration());
            modelBuilder.ApplyConfiguration(new TagConfiguration());
        }
    }
}
