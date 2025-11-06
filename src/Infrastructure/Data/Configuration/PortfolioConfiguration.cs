using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Configuration
{
    public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
    {
        public void Configure(EntityTypeBuilder<Portfolio> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Owner).IsRequired();

            builder.HasMany(p => p.Accounts)
                   .WithOne(a => a.Portfolio)
                   .HasForeignKey(a => a.PortfolioId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
