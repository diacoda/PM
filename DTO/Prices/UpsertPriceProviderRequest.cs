namespace PM.DTO.Prices;

public class UpsertPriceProviderRequest
{
    public string Symbol { get; set; } = default!;
    public DateOnly Date { get; set; }
}