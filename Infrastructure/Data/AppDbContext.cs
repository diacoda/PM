using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;
using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PM.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Portfolio> Portfolios { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        public DbSet<Holding> Holdings { get; set; } = null!;
        public DbSet<Tag> Tags { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
