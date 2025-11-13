namespace PM.Domain.Values;

public class SymbolFetchDetail
{
    public string Symbol { get; }
    public string Exchange { get; }
    public string Status { get; }
    public string? Error { get; }

    public SymbolFetchDetail(string symbol, string exchange, string status, string? error = null)
    {
        Symbol = symbol;
        Exchange = exchange;
        Status = status;
        Error = error;
    }
}
