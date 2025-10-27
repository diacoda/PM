using PM.Domain.Values;

namespace PM.Domain.Values;

public class BenchmarkDefinition
{
    public int Id { get; set; }           // consistent with your other entity-like types
    public string Name { get; set; } = string.Empty;
    public Currency ReportingCurrency { get; set; } = Currency.CAD;
    public IReadOnlyList<BenchmarkComponent> Components { get; init; } = Array.Empty<BenchmarkComponent>();
    public string RebalancePolicy { get; init; } = "Daily";   // doc string only (KISS)

    public decimal TotalWeight => Components.Sum(c => c.Weight);

    public BenchmarkDefinition(string name, Currency reportingCurrency, IReadOnlyList<BenchmarkComponent> components, string rebalancePolicy = "Daily")
    {
        Name = name;
        ReportingCurrency = reportingCurrency;
        Components = components;
        RebalancePolicy = rebalancePolicy;
    }
}