namespace PM.DTO.Prices;

public class UpdatePriceRequest
{
    public DateOnly Date { get; set; }
    public decimal Close { get; set; }
}