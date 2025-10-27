using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> b)
    {
        b.HasKey(p => p.Id);

        b.Property(p => p.Owner)
            .HasMaxLength(100)
            .IsRequired();

        // Proper one-to-many
        b.HasMany(p => p.Accounts)
            .WithOne(a => a.Portfolio)
            .HasForeignKey(a => a.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
