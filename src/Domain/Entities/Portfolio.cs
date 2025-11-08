using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PM.SharedKernel;

namespace PM.Domain.Entities
{
    /// <summary>
    /// Represents a portfolio containing multiple accounts owned by a single individual.
    /// </summary>
    public class Portfolio : Entity
    {
        /// <summary>
        /// Gets or sets the name of the portfolio owner.
        /// </summary>
        public string Owner { get; set; } = string.Empty;

        private readonly List<Account> _accounts = new();

        /// <summary>
        /// Gets the read-only collection of accounts contained in this portfolio.
        /// </summary>
        public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

        /// <summary>
        /// EF Core requires a parameterless constructor for materialization.
        /// </summary>
        public Portfolio() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Portfolio"/> class with a specified owner.
        /// </summary>
        /// <param name="owner">The name of the portfolio owner.</param>
        /// <exception cref="ArgumentException">Thrown when the owner name is null or whitespace.</exception>
        public Portfolio(string owner)
        {
            if (string.IsNullOrWhiteSpace(owner))
                throw new ArgumentException("Owner name is required.", nameof(owner));

            Owner = owner.Trim();
        }

        /// <summary>
        /// Adds an account to this portfolio.
        /// </summary>
        /// <param name="account">The account to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the account is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the account is already in the portfolio.</exception>
        public void AddAccount(Account account)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            if (_accounts.Any(a => a.Id == account.Id))
                throw new InvalidOperationException($"Account with ID {account.Id} is already in this portfolio.");

            account.LinkToPortfolio(this);
            _accounts.Add(account);
        }

        /// <summary>
        /// Removes an account from this portfolio.
        /// </summary>
        /// <param name="account">The account to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when the account is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the account is not found in the portfolio.</exception>
        public void RemoveAccount(Account account)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            if (!_accounts.Remove(account))
                throw new InvalidOperationException("Account not found in portfolio.");
        }

        /// <summary>
        /// Determines whether this portfolio contains an account with the specified ID.
        /// </summary>
        /// <param name="accountId">The ID of the account to check.</param>
        /// <returns><c>true</c> if the account exists in the portfolio; otherwise, <c>false</c>.</returns>
        public bool ContainsAccount(int accountId)
        {
            return _accounts.Any(a => a.Id == accountId);
        }

        /// <summary>
        /// Returns a string representation of the portfolio.
        /// </summary>
        /// <returns>A string containing the owner's name and number of accounts.</returns>
        public override string ToString() => $"{Owner}'s Portfolio (Accounts: {_accounts.Count})";
    }
}
