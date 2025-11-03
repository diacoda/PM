using PM.Domain.Enums;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Entities
{
    public class Account : Entity
    {
        public Account(string name, Currency currency, FinancialInstitutions financialInstitution)
        {
            Name = name;
            Currency = currency;
            FinancialInstitution = financialInstitution;
        }

        public int Id { get; private set; }
        public string Name { get; set; } = string.Empty;
        public FinancialInstitutions FinancialInstitution { get; set; }
        public Currency Currency { get; set; } = new Currency("CAD");

        private readonly List<Holding> _holdings = new();
        public IReadOnlyCollection<Holding> Holdings => _holdings.AsReadOnly();

        private readonly List<Transaction> _transactions = new();
        public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

        private readonly List<Tag> _tags = new();
        public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

        // 🔹 Add this navigation + FK
        public int PortfolioId { get; private set; }   // foreign key
        public Portfolio Portfolio { get; private set; } = null!;

        public void UpdateName(string newName)
        {
            Name = newName;
        }

        public void AddTag(Tag tag)
        {
            if (!_tags.Contains(tag))
                _tags.Add(tag);
        }

        public void RemoveTag(Tag tag)
        {
            _tags.Remove(tag);
        }

        public Holding UpsertHolding(Holding holding)
        {
            var existing = _holdings.FirstOrDefault(h => h.Symbol.Equals(holding.Symbol));
            if (existing != null)
            {
                existing.AddQuantity(holding.Quantity); // merge quantities
                return existing;
            }
            else
            {
                _holdings.Add(holding);
                return holding;
            }
        }

        public Holding UpdateHoldingQuantity(Symbol symbol, decimal newQuantity)
        {
            var holding = _holdings.FirstOrDefault(h => h.Symbol.Equals(symbol));
            if (holding == null)
                throw new InvalidOperationException($"Holding not found for symbol {symbol.Code}");

            holding.UpdateQuantity(newQuantity);
            return holding;
        }

        public void RemoveHolding(Holding holding)
        {
            _holdings.Remove(holding);
        }

        public void AddTransaction(Transaction transaction)
        {
            _transactions.Add(transaction);
        }

        public void RemoveTransaction(Transaction transaction)
        {
            _transactions.Remove(transaction);
        }

        public decimal GetCashBalance(Currency currency)
        {
            var symbol = new Symbol("CAD");
            var holding = Holdings.FirstOrDefault(h => h.Symbol == symbol);
            return holding?.Quantity ?? 0;
        }
        public void LinkToPortfolio(Portfolio portfolio)
        {
            Portfolio = portfolio ?? throw new ArgumentNullException(nameof(portfolio));
            PortfolioId = portfolio.Id;
        }
        public override string ToString() =>
            $"Account {Id}: {Name} ({Currency}) @ {FinancialInstitution}";
    }
}
