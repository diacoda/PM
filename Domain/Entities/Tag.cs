namespace PM.Domain.Entities
{
    public class Tag
    {
        public int Id { get; set; } // Primary key for EF
        public string Name { get; private set; } = string.Empty;

        // EF constructor
        private Tag() { }

        public Tag(string name)
        {
            Name = name.Trim();
        }

        public override string ToString() => Name;

        public override bool Equals(object? obj) =>
            obj is Tag other && string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode() => Name.ToLowerInvariant().GetHashCode();
    }
}
