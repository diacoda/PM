using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Infrastructure.Data.Configuration;

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
            modelBuilder.ApplyConfiguration(new CashFlowConfiguration());

        }
    }
}
