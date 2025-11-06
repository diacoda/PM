using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Configuration
{
    public class HoldingConfiguration : IEntityTypeConfiguration<Holding>
    {
        public void Configure(EntityTypeBuilder<Holding> builder)
        {
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Quantity).IsRequired();

            // EF owns concrete Asset, domain exposes IAsset
            builder.OwnsOne<Asset>("_asset", sb =>
            {
                sb.Property(a => a.Code).HasColumnName("AssetCode").IsRequired();
                sb.Property(a => a.AssetClass).HasColumnName("AssetClass").IsRequired();
                sb.OwnsOne(a => a.Currency, cb =>
                {
                    cb.Property(c => c.Code).HasColumnName("AssetCurrency").IsRequired();
                });
            });

            // Tell EF to ignore domain IAsset property
            builder.Ignore(h => h.Asset);
        }
    }
}
