using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface IReportingService
{
    Dictionary<AssetClass, Money> AggregateByAssetClass(Account account, DateTime date, Currency reportingCurrency);
    Dictionary<AssetClass, Money> AggregateByAssetClass(Portfolio portfolio, DateTime date, Currency reportingCurrency);
    void PrintHoldingsSummary(Account account);
    void PrintTransactionHistory(Account account, DateTime from, DateTime to);
    Dictionary<AssetClass, decimal> GetAssetClassPercentages(Account account, DateTime date, Currency reportingCurrency);
    Dictionary<AssetClass, decimal> GetAssetClassPercentages(Portfolio portfolio, DateTime date, Currency reportingCurrency);
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