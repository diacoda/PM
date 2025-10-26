namespace model.Domain.Entities;

public class Portfolio
{
    private static int _nextId = 0;
    public Portfolio()
    {
        Id = Interlocked.Increment(ref _nextId);
    }
    public int Id { get; set; }
    public string Owner { get; set; } = String.Empty;
    public List<Account> Accounts { get; set; } = new();
}
