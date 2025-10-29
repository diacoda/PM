namespace PM.DTO.Prices;

/// <summary>
/// Represents the result of a price fetch operation for a specific date.
/// </summary>
/// <param name="Date">The date for which prices were fetched.</param>
/// <param name="Fetched">List of symbols that were successfully fetched.</param>
/// <param name="Skipped">List of symbols that were skipped (e.g., already up to date).</param>
/// <param name="Errors">List of symbols that failed to fetch with errors.</param>
public record FetchPricesDTO(
    DateOnly Date,
    List<string> Fetched,
    List<string> Skipped,
    List<string> Errors
);
