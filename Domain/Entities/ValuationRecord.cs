namespace PM.Domain.Entities;

using PM.Domain.Enums;
using PM.Domain.Values;
using PM.SharedKernel;

public class ValuationRecord : Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public ValuationPeriod Period { get; set; }
    public Currency ReportingCurrency { get; set; } = Currency.CAD;
    public Money Value { get; set; } = new Money(0.0m, Currency.CAD);

    // Optional references
    public int? AccountId { get; set; }
    public int? PortfolioId { get; set; }
    public AssetClass? AssetClass { get; set; }

    // Already present for asset-class slices
    public decimal? Percentage { get; set; }

    // NEW: explicit components to “cleanly separate” the snapshot
    public Money? SecuritiesValue { get; set; }  // total ex-cash, in ReportingCurrency
    public Money? CashValue { get; set; }        // total cash, in ReportingCurrency
    public Money? IncomeForDay { get; set; }     // e.g., dividends recorded that day (net), in ReportingCurrency
}