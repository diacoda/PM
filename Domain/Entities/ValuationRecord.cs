namespace PM.Domain.Entities;

using PM.Domain.Enums;
using PM.Domain.Values;
using PM.SharedKernel;

/// <summary>
/// Represents a snapshot of the valuation of a portfolio or account at a specific date and period.
/// Can include total value, cash, securities, and income components.
/// </summary>
public class ValuationRecord : Entity
{
    /// <summary>
    /// Gets or sets the unique identifier of the valuation record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the date for which this valuation applies.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets the valuation period (e.g., Daily, Monthly, Yearly).
    /// </summary>
    public ValuationPeriod Period { get; set; }

    /// <summary>
    /// Gets or sets the currency in which the valuation is reported.
    /// </summary>
    public Currency ReportingCurrency { get; set; } = Currency.CAD;

    /// <summary>
    /// Gets or sets the total value of the portfolio/account in the reporting currency.
    /// </summary>
    public Money Value { get; set; } = new Money(0.0m, Currency.CAD);

    /// <summary>
    /// Gets or sets the optional account ID this valuation is associated with.
    /// </summary>
    public int? AccountId { get; set; }

    /// <summary>
    /// Gets or sets the optional portfolio ID this valuation is associated with.
    /// </summary>
    public int? PortfolioId { get; set; }

    /// <summary>
    /// Gets or sets the optional asset class for which this valuation record is a slice.
    /// </summary>
    public AssetClass? AssetClass { get; set; }

    /// <summary>
    /// Gets or sets the optional percentage of the total value represented by this slice.
    /// Useful when recording asset-class breakdowns.
    /// </summary>
    public decimal? Percentage { get; set; }

    /// <summary>
    /// Gets or sets the value of securities (excluding cash) in the reporting currency.
    /// </summary>
    public Money? SecuritiesValue { get; set; }

    /// <summary>
    /// Gets or sets the value of cash in the reporting currency.
    /// </summary>
    public Money? CashValue { get; set; }

    /// <summary>
    /// Gets or sets the income earned for the day (e.g., dividends), net of fees, in the reporting currency.
    /// </summary>
    public Money? IncomeForDay { get; set; }
}
