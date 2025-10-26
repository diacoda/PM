using model.Domain.Entities;
using model.Domain.Values;

namespace model.Services;

public class BenchmarkService
{
    private readonly ValuationService _valuation;

    public BenchmarkService(ValuationService valuation)
    {
        _valuation = valuation;
    }

    /// <summary>
    /// Daily rebalanced benchmark:
    /// r_bench(d) = Σ w_i * r_i(d), where r_i are component security daily returns in reporting currency.
    /// Emits DailyReturn with EntityType=Benchmark and EntityId=def.Id.
    /// </summary>
    public IEnumerable<DailyReturn> GetDailyBenchmarkReturns(BenchmarkDefinition def, DateTime start, DateTime end)
    {
        if (def.Components == null || def.Components.Count == 0) yield break;

        var totalW = def.TotalWeight;
        if (totalW == 0m) yield break;

        var parts = def.Components.Select(c => (c.Instrument, w: c.Weight / totalW)).ToList();
        var holdings = parts.Select(p => new Holding { Instrument = p.Instrument, Quantity = 1m }).ToList();

        for (var d = start.Date.AddDays(1); d <= end.Date; d = d.AddDays(1))
        {
            var prev = d.AddDays(-1);
            decimal rBench = 0m;

            for (int i = 0; i < holdings.Count; i++)
            {
                var h = holdings[i];
                var w = parts[i].w;

                var v0 = _valuation.CalculateHoldingValue(h, prev, def.ReportingCurrency).Amount;
                var v1 = _valuation.CalculateHoldingValue(h, d,    def.ReportingCurrency).Amount;

                var ri = v0 == 0m ? 0m : (v1 - v0) / v0;
                rBench += w * ri;
            }

            yield return new DailyReturn(
                Date: d,
                EntityType: EntityKind.Benchmark,
                EntityId: def.Id,
                ReportingCurrency: def.ReportingCurrency,
                Return: rBench
            );
        }
    }
}