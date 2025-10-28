namespace PM.Domain.Values
{
    public sealed record Currency
    {
        public string Code { get; private set; } = String.Empty;

        private Currency() { } // EF

        public Currency(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new System.ArgumentException("Currency code is required.", nameof(code));

            code = code.Trim().ToUpperInvariant();

            if (code.Length != 3)
                throw new System.ArgumentException("Currency code must be 3 characters (ISO 4217).", nameof(code));

            Code = code;
        }
        public static Currency CAD => new("CAD");
        public static Currency USD => new("USD");
        public static Currency EUR => new("EUR");

        public override string ToString() => Code;
    }
}