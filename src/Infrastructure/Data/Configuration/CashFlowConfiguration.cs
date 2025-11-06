using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Infrastructure.Data.Configuration
{
    public class CashFlowConfiguration : IEntityTypeConfiguration<CashFlow>
    {
        public void Configure(EntityTypeBuilder<CashFlow> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.AccountId).IsRequired();
            builder.Property(c => c.Type).HasConversion<int>().IsRequired();
            builder.Property(c => c.Note).HasMaxLength(200);

            builder.OwnsOne(c => c.Amount, mb => mb.ConfigureMoney());
        }
    }
}
