using PM.SharedKernel;

namespace PM.Domain.Entities;

public class Tag : Entity
{
    public int Id { get; set; } // Primary key for EF
    public string Name { get; private set; } = string.Empty;

    // EF constructor
    private Tag() { }

    public Tag(string name)
    {
        Name = name.Trim();
    }
    
    public void UpdateName(string newName)
    {
        Name = newName?.Trim() ?? string.Empty;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj) =>
        obj is Tag other && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

    public override int GetHashCode() => Name.ToLowerInvariant().GetHashCode();
}
