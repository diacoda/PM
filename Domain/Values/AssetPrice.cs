namespace PM.Domain.Values
{
    public class AssetPrice
    {
        private AssetPrice() { }

        public AssetPrice(Symbol symbol, DateOnly date, Money price, string source = "Manual")
        {
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            Date = date;
            Price = price ?? throw new ArgumentNullException(nameof(price));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            CreatedAtUtc = DateTimeOffset.UtcNow;
        }

        public Symbol Symbol { get; private set; } = default!;

        public DateOnly Date { get; private set; }

        public Money Price { get; private set; } = default!;

        public string Source { get; set; } = string.Empty;

        public DateTimeOffset CreatedAtUtc { get; private set; }
    }
}
