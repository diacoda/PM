using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Entities;

public class Holding : Entity
{
    // EF requires parameterless constructor
    private Holding() { } // <- EF Core uses this

    // Convenience constructor for domain usage
    public Holding(Instrument instrument, decimal quantity)
    {
        Instrument = instrument ?? throw new ArgumentNullException(nameof(instrument));
        Quantity = quantity;
    }

    public int Id { get; private set; }

    public Instrument Instrument { get; set; } = default!;

    public decimal Quantity { get; set; }

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public List<Tag> Tags { get; set; } = new();

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
