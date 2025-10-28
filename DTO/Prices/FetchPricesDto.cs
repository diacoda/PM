namespace PM.DTO.Prices;

public record FetchPricesDTO(
    DateOnly Date,
    List<string> Fetched,
    List<string> Skipped,
    List<string> Errors
);
