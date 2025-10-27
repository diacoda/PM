using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        var currencyConverter = new ValueConverter<Currency, string>(
            v => v.Code,
            v => new Currency(v));

        b.HasKey(a => a.Id);

        b.Property(a => a.Name)
            .HasMaxLength(100)
            .IsRequired();

        b.Property(a => a.FinancialInstitution)
            .HasConversion<int>() // store enum as int
            .IsRequired();

        b.Property(a => a.Currency)
            .HasConversion(currencyConverter)
            .HasMaxLength(3)
            .IsRequired();

        b.HasMany(a => a.Holdings)
            .WithOne(h => h.Account)
            .HasForeignKey(h => h.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-many: Account ↔ Tags
        b.HasMany<Tag>()
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "AccountTag",
                j => j.HasOne<Tag>().WithMany().HasForeignKey("TagId"),
                j => j.HasOne<Account>().WithMany().HasForeignKey("AccountId"));
    }
}
