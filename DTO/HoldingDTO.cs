namespace PM.DTO;

public class HoldingDTO
{
    public int Id { get; set; }

    public string Symbol { get; set; } = string.Empty;

    public string InstrumentName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public int AccountId { get; set; }

    public List<string> Tags { get; set; } = new();
}
