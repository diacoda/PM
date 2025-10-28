using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Configurations;

public class HoldingConfiguration : IEntityTypeConfiguration<Holding>
{
    public void Configure(EntityTypeBuilder<Holding> b)
    {
        b.HasKey(h => h.Id);

        b.Property(h => h.Quantity)
            .IsRequired();

        // ✅ Owned type mapping for Symbol
        b.OwnsOne(h => h.Symbol, s =>
        {
            s.Property(x => x.Value)
                .HasMaxLength(20)
                .IsRequired();

            s.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            s.Property(x => x.Exchange)
                .HasMaxLength(10)
                .IsRequired();

            s.Property(x => x.AssetClass)
                .HasConversion<int>()
                .IsRequired();
        });

        // ✅ Many-to-many for Tags
        b.HasMany(h => h.Tags)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "HoldingTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<Holding>().WithMany().HasForeignKey("HoldingId"));
    }
}