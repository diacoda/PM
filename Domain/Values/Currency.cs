namespace PM.Domain.Values
{
    public class Currency
    {
        public string Code { get; private set; }

        private Currency() { } // EF

        public Currency(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Currency code cannot be empty.", nameof(code));

            Code = code.ToUpperInvariant();
        }
        public static Currency CAD => new("CAD");
        public static Currency USD => new("USD");
        public static Currency EUR => new("EUR");

        public override string ToString() => Code;
    }
}
