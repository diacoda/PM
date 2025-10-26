using model.Domain.Values;

namespace model.Domain.Entities;

public class Holding
{
    public Instrument Instrument { get; set; } = new Instrument(Symbol.From(""), "", AssetClass.Other);
    public decimal Quantity { get; set; }
    public List<Tag> Tags { get; set; } = new();
    public void AddTag(Tag tag)
    {
        
    }
}
