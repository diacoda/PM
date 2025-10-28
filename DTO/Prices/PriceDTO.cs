namespace PM.DTO.Prices;

public class PriceDTO
{
    public string Symbol { get; set; } = default!;
    public DateOnly Date { get; set; }
    public decimal Close { get; set; }
}