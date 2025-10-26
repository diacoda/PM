using model.Domain.Values;

namespace model.Domain.Entities;

public class Account
{
    private static int _nextId = 0;
    public Account()
    {
        Id = Interlocked.Increment(ref _nextId);
    }
    public int Id { get; private set; }
    public string Name { get; set; } = String.Empty;
    public FinancialInstitutions FinancialInstitution { get; set; }
    public Currency Currency { get; set; } = new Currency("CAD");

    public readonly List<Holding> _holdings = new();
    public IReadOnlyCollection<Holding> Holdings => _holdings.AsReadOnly();
    public readonly List<Transaction> _transactions = new();
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();
    public readonly List<Tag> _tags = new();
    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public override string ToString()
    {
        return $"Account Id: {Id}, Name: {Name}, Currency: {Currency} @ {FinancialInstitution.ToString()}";
    }

    public void AddTag(Tag tag)
    {
        if (!_tags.Contains(tag))
        {
            _tags.Add(tag);
        }
    }
    public void RemoveTag(Tag tag)
    {
        if (_tags.Contains(tag))
        {
            _tags.Remove(tag);
        }
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
}
