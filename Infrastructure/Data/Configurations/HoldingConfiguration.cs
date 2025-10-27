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

        // ✅ Owned type mapping for Instrument
        b.OwnsOne(h => h.Instrument, i =>
        {
            i.Property(x => x.Name)
                .HasMaxLength(100);

            i.Property(x => x.AssetClass)
                .HasConversion<int>();

            i.OwnsOne(x => x.Symbol, s =>
            {
                s.Property(x => x.Value)
                    .HasMaxLength(20)
                    .IsRequired();
            });
        });

        // ✅ Many-to-many for Tags
        b.HasMany<Tag>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "HoldingTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<Holding>().WithMany().HasForeignKey("HoldingId"));
    }
}
