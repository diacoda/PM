using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;
using PM.Domain.Values;
namespace PM.Infrastructure.Data.Configuration;

public class CashFlowConfiguration : IEntityTypeConfiguration<CashFlow>
{
    public void Configure(EntityTypeBuilder<CashFlow> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.AccountId)
               .IsRequired();

        builder.Property(c => c.Type)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(c => c.Note)
               .HasMaxLength(200);

        // Configure owned type Money
        builder.OwnsOne(c => c.Amount, m =>
        {
            m.Property(x => x.Amount)
             .HasColumnType("decimal(18,4)")
             .IsRequired();

            m.Property(x => x.Currency)
             .HasConversion(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<Currency, string>(
                 c => c.Code,
                 s => new Currency(s)))
             .HasMaxLength(3)
             .IsRequired();
        });
    }
}
