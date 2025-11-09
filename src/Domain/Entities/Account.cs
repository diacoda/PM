using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PM.Domain.Enums;
using PM.Domain.Interfaces;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Entities
{
    /// <summary>
    /// Represents a financial account within a portfolio.
    /// Holds information about holdings, transactions, currency, and associated tags.
    /// </summary>
    public class Account : Entity
    {
        private Account() { }
        public Account(string name, Currency currency, FinancialInstitutions financialInstitution)
        {
            Name = name;
            Currency = currency;
            CurrencyCode = currency.Code;
            FinancialInstitution = financialInstitution;
        }

        public string Name { get; set; } = string.Empty;

        public FinancialInstitutions FinancialInstitution { get; set; }

        [NotMapped]
        public Currency Currency { get; private set; } = new Currency("CAD");
        public string CurrencyCode
        {
            get => Currency.Code;
            private set => Currency = new Currency(value);
        }

        /// <summary>
        /// Backing list of holdings.
        /// </summary>
        private readonly List<Holding> _holdings = new();

        /// <summary>
        /// Gets the collection of holdings in this account.
        /// </summary>
        public IReadOnlyCollection<Holding> Holdings => _holdings.AsReadOnly();

        /// <summary>
        /// Backing list of transactions.
        /// </summary>
        private readonly List<Transaction> _transactions = new();

        /// <summary>
        /// Gets the collection of transactions for this account.
        /// </summary>
        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

        /// <summary>
        /// Backing list of tags associated with this account.
        /// </summary>
        private readonly List<Tag> _tags = new();

        /// <summary>
        /// Gets the collection of tags associated with this account.
        /// </summary>
        public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

        /// <summary>
        /// Foreign key to the parent portfolio.
        /// </summary>
        public int PortfolioId { get; private set; }

        /// <summary>
        /// Navigation property to the parent portfolio.
        /// </summary>
        public Portfolio Portfolio { get; private set; } = null!;

        /// <summary>
        /// Updates the account's display name.
        /// </summary>
        /// <param name="newName">The new account name.</param>
        public void UpdateName(string newName)
        {
            Name = newName;
        }

        /// <summary>
        /// Adds a tag to the account if it doesn't already exist.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        public void AddTag(Tag tag)
        {
            if (!_tags.Contains(tag))
                _tags.Add(tag);
        }

        /// <summary>
        /// Removes a tag from the account.
        /// </summary>
        /// <param name="tag">The tag to remove.</param>
        public void RemoveTag(Tag tag)
        {
            _tags.Remove(tag);
        }

        /// <summary>
        /// Adds or updates a holding in the account.
        /// If a holding with the same symbol exists, it merges the quantities.
        /// </summary>
        /// <param name="holding">The holding to upsert.</param>
        /// <returns>The updated or newly added holding.</returns>
        public Holding UpsertHolding(Holding holding)
        {
            var existing = _holdings.FirstOrDefault(h => h.Asset.Equals(holding.Asset));
            if (existing != null)
            {
                existing.AddQuantity(holding.Quantity);
                return existing;
            }
            else
            {
                _holdings.Add(holding);
                return holding;
            }
        }

        /// <summary>
        /// Updates the quantity of an existing holding.
        /// </summary>
        /// <param name="symbol">The symbol of the holding to update.</param>
        /// <param name="newQuantity">The new quantity to set.</param>
        /// <returns>The updated holding.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the holding is not found.</exception>
        public Holding UpdateHoldingQuantity(IAsset asset, decimal newQuantity)
        {
            var holding = _holdings.FirstOrDefault(h => h.Asset.Equals(asset));
            if (holding == null)
                throw new InvalidOperationException($"Holding not found for asset {asset.Code}");

            holding.UpdateQuantity(newQuantity);
            return holding;
        }

        /// <summary>
        /// Removes a holding from the account.
        /// </summary>
        /// <param name="holding">The holding to remove.</param>
        public void RemoveHolding(Holding holding)
        {
            _holdings.Remove(holding);
        }

        /// <summary>
        /// Adds a transaction to the account.
        /// </summary>
        /// <param name="transaction">The transaction to add.</param>
        public void AddTransaction(Transaction transaction)
        {
            _transactions.Add(transaction);
        }

        /// <summary>
        /// Removes a transaction from the account.
        /// </summary>
        /// <param name="transaction">The transaction to remove.</param>
        public void RemoveTransaction(Transaction transaction)
        {
            _transactions.Remove(transaction);
        }

        /// <summary>
        /// Returns the cash balance for a given currency.
        /// </summary>
        /// <param name="currency">The currency to check.</param>
        /// <returns>The quantity of cash in the account for the given currency.</returns>
        public decimal GetCashBalance(Currency currency)
        {
            var symbol = new Symbol("CAD");
            var holding = Holdings.FirstOrDefault(h => h.Asset?.Equals(symbol) == true);
            return holding?.Quantity ?? 0;
        }

        /// <summary>
        /// Links this account to a parent portfolio.
        /// </summary>
        /// <param name="portfolio">The portfolio to link to.</param>
        public void LinkToPortfolio(Portfolio portfolio)
        {
            Portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));
            PortfolioId = portfolio.Id;
        }

        /// <summary>
        /// Returns a string representation of the account.
        /// </summary>
        public override string ToString() =>
            $"Account {Id}: {Name} ({Currency}) @ {FinancialInstitution}";
    }
}
