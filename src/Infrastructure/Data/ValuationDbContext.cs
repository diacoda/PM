using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Infrastructure.Data.Configuration;

namespace PM.Infrastructure.Data
{
    public class ValuationDbContext : DbContext
    {
        public ValuationDbContext(DbContextOptions<ValuationDbContext> options)
            : base(options) { }

        public DbSet<ValuationRecord> ValuationRecords => Set<ValuationRecord>();
        public DbSet<AssetPrice> Prices => Set<AssetPrice>();
        public DbSet<FxRate> FxRates => Set<FxRate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new ValuationRecordConfiguration());
            modelBuilder.ApplyConfiguration(new FxRateConfiguration());
            modelBuilder.ApplyConfiguration(new AssetPriceConfiguration());
        }
    }
}
