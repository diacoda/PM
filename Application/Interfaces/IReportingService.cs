using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IReportingService
{
    Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Account account, DateTime date, Currency reportingCurrency);
    Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Portfolio portfolio, DateTime date, Currency reportingCurrency);
    void PrintHoldingsSummary(Account account);
    void PrintTransactionHistory(Account account, DateTime from, DateTime to);
    Task<Dictionary<AssetClass, decimal>> GetAssetClassPercentagesAsync(Account account, DateTime date, Currency reportingCurrency);
    Task<Dictionary<AssetClass, decimal>> GetAssetClassPercentagesAsync(Portfolio portfolio, DateTime date, Currency reportingCurrency);
    Dictionary<Currency, decimal> GetTradingCostsByCurrency(Account account, DateTime from, DateTime to);
    Dictionary<Currency, decimal> GetTradingCostsByCurrency(Portfolio portfolio, DateTime from, DateTime to);
    IEnumerable<TransactionCostSummary> GetTransactionCostSummaries(Account account, DateTime from, DateTime to);
    IEnumerable<TransactionCostSummary> GetTransactionCostSummaries(Portfolio portfolio, DateTime from, DateTime to);
    IEnumerable<(string Symbol, Currency Currency, decimal TotalCosts, decimal Gross, TransactionType Type)>
        GetTransactionCostsBySecurity(Account account, DateTime from, DateTime to);
    IEnumerable<(string Symbol, Currency Currency, decimal TotalCosts, decimal Gross, TransactionType Type)>
        GetTransactionCostsBySecurity(Portfolio portfolio, DateTime from, DateTime to);
    void PrintTransactionCostReport(Account account, DateTime from, DateTime to);
    void PrintTransactionCostReport(Portfolio portfolio, DateTime from, DateTime to);
}