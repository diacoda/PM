using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Domain.Entities
{
    public class Account
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

        public void AddHolding(Holding holding)
        {
            _holdings.Add(holding);
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
            var symbol = Symbol.From($"CASH.{currency}");
            var holding = Holdings.FirstOrDefault(h => h.Instrument.Symbol == symbol);
            return holding?.Quantity ?? 0;
        }

        public override string ToString() =>
            $"Account {Id}: {Name} ({Currency}) @ {FinancialInstitution}";
    }
}
