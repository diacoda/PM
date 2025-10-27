namespace PM.Domain.Values
{
    public class Symbol
    {
        public string Value { get; private set; }

        private Symbol() { } // EF

        private Symbol(string value)
        {
            Value = value.Trim().ToUpperInvariant();
        }

        public static Symbol From(string value) => new Symbol(value);

        public override string ToString() => Value;

        public override bool Equals(object? obj) =>
            obj is Symbol other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }
}