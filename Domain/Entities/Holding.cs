using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Entities;

public class Holding : Entity
{
    // EF requires parameterless constructor
    private Holding() { } // <- EF Core uses this

    // Convenience constructor for domain usage
    public Holding(Symbol symbol, decimal quantity)
    {
        Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
        Quantity = quantity;
    }

    public int Id { get; private set; }

    public Symbol Symbol { get; set; } = default!;

    public decimal Quantity { get; set; }

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public List<Tag> Tags { get; set; } = new();

    public void AddQuantity(decimal qty)
    {
        Quantity += qty;
    }

    public void UpdateQuantity(decimal newQuantity)
    {
        Quantity = newQuantity;
    }

    public void AddTag(Tag tag)
    {
        if (!Tags.Contains(tag))
            Tags.Add(tag);
    }

    public void RemoveTag(Tag tag)
    {
        Tags.Remove(tag);
    }
}
