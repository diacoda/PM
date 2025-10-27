using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PM.Domain.Entities
{
    public class Portfolio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        public string Owner { get; set; } = string.Empty;

        private readonly List<Account> _accounts = new();
        public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

        // EF Core requires a parameterless constructor
        public Portfolio() { }

        public Portfolio(string owner)
        {
            if (string.IsNullOrWhiteSpace(owner))
                throw new ArgumentException("Owner name is required.", nameof(owner));

            Owner = owner.Trim();
        }

        public void AddAccount(Account account)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            if (_accounts.Any(a => a.Id == account.Id))
                throw new InvalidOperationException($"Account with ID {account.Id} is already in this portfolio.");

            _accounts.Add(account);
        }

        public void RemoveAccount(Account account)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            if (!_accounts.Remove(account))
                throw new InvalidOperationException("Account not found in portfolio.");
        }

        public bool ContainsAccount(int accountId)
        {
            return _accounts.Any(a => a.Id == accountId);
        }

        public override string ToString() => $"{Owner}'s Portfolio (Accounts: {_accounts.Count})";
    }
}
