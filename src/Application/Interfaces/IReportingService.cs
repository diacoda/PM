using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IReportingService
{
    Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Account account, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Portfolio portfolio, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    void PrintHoldingsSummary(Account account);
    void PrintTransactionHistory(Account account, DateOnly from, DateOnly to);
    Task<Dictionary<AssetClass, decimal>> GetAssetClassPercentagesAsync(Account account, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Task<Dictionary<AssetClass, decimal>> GetAssetClassPercentagesAsync(Portfolio portfolio, DateOnly date, Currency reportingCurrency, CancellationToken ct = default);
    Dictionary<Currency, decimal> GetTradingCostsByCurrency(Account account, DateOnly from, DateOnly to);
    Dictionary<Currency, decimal> GetTradingCostsByCurrency(Portfolio portfolio, DateOnly from, DateOnly to);
    IEnumerable<TransactionCostSummary> GetTransactionCostSummaries(Account account, DateOnly from, DateOnly to);
    IEnumerable<TransactionCostSummary> GetTransactionCostSummaries(Portfolio portfolio, DateOnly from, DateOnly to);
    IEnumerable<(string Symbol, Currency Currency, decimal TotalCosts, decimal Gross, TransactionType Type)>
        GetTransactionCostsBySecurity(Account account, DateOnly from, DateOnly to);
    IEnumerable<(string Symbol, Currency Currency, decimal TotalCosts, decimal Gross, TransactionType Type)>
        GetTransactionCostsBySecurity(Portfolio portfolio, DateOnly from, DateOnly to);
    void PrintTransactionCostReport(Account account, DateOnly from, DateOnly to);
    void PrintTransactionCostReport(Portfolio portfolio, DateOnly from, DateOnly to);
}