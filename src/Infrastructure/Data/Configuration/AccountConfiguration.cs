using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PM.Domain.Entities;

namespace PM.Infrastructure.Data.Configuration
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.Name).IsRequired();
            builder.Property(a => a.CurrencyCode).IsRequired();

            builder.HasMany(a => a.Holdings)
                   .WithOne(h => h.Account)
                   .HasForeignKey(h => h.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.Transactions)
                   .WithOne(t => t.Account)
                   .HasForeignKey(t => t.AccountId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
