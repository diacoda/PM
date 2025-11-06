using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Values;

namespace PM.Infrastructure.Data.Configuration
{
    public class AssetPriceConfiguration : IEntityTypeConfiguration<AssetPrice>
    {
        public void Configure(EntityTypeBuilder<AssetPrice> builder)
        {
            var symbolConverter = ValueConverters.SymbolStringConverter;

            builder.HasKey(p => new { p.Symbol, p.Date });

            builder.Property(p => p.Symbol)
                   .HasConversion(symbolConverter)
                   .HasMaxLength(24)
                   .IsRequired();

            builder.Property(p => p.Date).IsRequired();

            builder.OwnsOne(p => p.Price, mb => mb.ConfigureMoney("Price", "Currency"));

            builder.Property(p => p.Source).IsRequired();
            builder.Property(p => p.CreatedAtUtc).IsRequired();
        }
    }
}
