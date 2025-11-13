namespace PM.DTO.Prices;

/// <summary>
/// Represents the result of a price fetch operation for a specific date.
/// </summary>
/// <param name="Date">The date for which prices were fetched.</param>
/// <param name="Fetched">List of symbols that were successfully fetched.</param>
/// <param name="Skipped">List of symbols that were skipped (e.g., already up to date).</param>
/// <param name="Errors">List of symbols that failed to fetch with errors.</param>
public class FetchPricesDTO
{
    public DateOnly Date { get; }
    public List<string> Fetched { get; }
    public List<string> Skipped { get; }
    public List<string> Errors { get; }
    public List<SymbolFetchDetailDTO> Details { get; }

    public FetchPricesDTO(DateOnly date, List<string> fetched, List<string> skipped, List<string> errors, List<SymbolFetchDetailDTO> details)
    {
        Date = date;
        Fetched = fetched;
        Skipped = skipped;
        Errors = errors;
        Details = details;
    }
}