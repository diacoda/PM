namespace PM.DTO;
public class ValuationRecordDTO
{
    public DateOnly Date { get; set; }
    public string Period { get; set; }
    public string ReportingCurrency { get; set; }
    public decimal Value { get; set; }
    public string ValueCurrency { get; set; }
    public int? AccountId { get; set; }
    public int? PortfolioId { get; set; }
    public string? AssetClass { get; set; }
    public decimal? Percentage { get; set; }
    public decimal? SecuritiesValue { get; set; }
    public string? SecuritiesValueCurrency { get; set; }
    public decimal? CashValue { get; set; }
    public string? CashValueCurrency { get; set; }
    public decimal? IncomeForDay { get; set; }
    public string? IncomeForDayCurrency { get; set; }
}
