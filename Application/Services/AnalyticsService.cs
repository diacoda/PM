using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    public decimal Link(IEnumerable<decimal> dailyReturns)
        => dailyReturns.Aggregate(1m, (acc, r) => acc * (1m + r)) - 1m;

    public RollingReturnSet ComputeRolling(DailyReturn[] series, DateTime asOf, DateTime seriesStart)
    {
        var arr = series.OrderBy(x => x.Date).ToArray();

        // trading-day approximations for simplicity
        int d1M = 21, d3M = 63, d6M = 126, d1Y = 252, d3Y = 756;

        decimal GetLinkedByDays(int days)
        {
            var from = asOf.AddDays(-days);
            var slice = arr.Where(x => x.Date > from && x.Date <= asOf).Select(x => x.Return);
            return Link(slice);
        }

        var ytdStart = new DateTime(asOf.Year, 1, 1);
        var rYtd = Link(arr.Where(x => x.Date >= ytdStart && x.Date <= asOf).Select(x => x.Return));
        var rSi = Link(arr.Where(x => x.Date >= seriesStart && x.Date <= asOf).Select(x => x.Return));

        return new RollingReturnSet(
            asOf,
            R_1M: GetLinkedByDays(d1M),
            R_3M: GetLinkedByDays(d3M),
            R_6M: GetLinkedByDays(d6M),
            R_YTD: rYtd,
            R_1Y: GetLinkedByDays(d1Y),
            R_3Y: GetLinkedByDays(d3Y),
            R_SI: rSi
        );
    }

    public RiskCard ComputeRisk(DailyReturn[] series, DailyReturn[]? benchmark = null)
    {
        var arr = series.OrderBy(x => x.Date).ToArray();
        if (arr.Length == 0) return new RiskCard(0, 0, null, null, 0, 0, null);

        var rets = arr.Select(x => x.Return).ToArray();
        var stdevDaily = StdDev(rets);
        var volAnnual = stdevDaily * (decimal)Math.Sqrt(252);

        var sharpe = volAnnual == 0 ? 0 : (Link(rets) /*approx over period*/) / volAnnual;
        var hit = (decimal)rets.Count(r => r > 0m) / rets.Length;

        var (mdd, peak, trough) = MaxDrawdown(arr);

        decimal? corr = null;
        if (benchmark != null && benchmark.Length > 0)
            corr = Correlation(AlignByDate(arr, benchmark.OrderBy(x => x.Date).ToArray()));

        return new RiskCard(volAnnual, mdd, peak, trough, sharpe, hit, corr);
    }

    public Dictionary<(int Year, int Month), decimal> CalendarMonthlyReturns(DailyReturn[] series)
    {
        var arr = series.OrderBy(x => x.Date).ToArray();
        return arr
            .GroupBy(x => (x.Date.Year, x.Date.Month))
            .ToDictionary(
                g => g.Key,
                g => Link(g.Select(s => s.Return))
            );
    }

    // ----- helpers -----

    private static ((decimal[] X, decimal[] Y) aligned, DateTime[] dates) AlignByDate(DailyReturn[] a, DailyReturn[] b)
    {
        var dictA = a.ToDictionary(x => x.Date, x => x.Return);
        var dictB = b.ToDictionary(x => x.Date, x => x.Return);

        var common = dictA.Keys.Intersect(dictB.Keys).OrderBy(d => d).ToArray();
        var xa = new decimal[common.Length];
        var xb = new decimal[common.Length];
        for (int i = 0; i < common.Length; i++)
        {
            xa[i] = dictA[common[i]];
            xb[i] = dictB[common[i]];
        }
        return ((xa, xb), common);
    }

    private static decimal Correlation(((decimal[] X, decimal[] Y) aligned, DateTime[] dates) data)
    {
        var x = data.aligned.X; var y = data.aligned.Y;
        if (x.Length == 0) return 0m;
        var mx = x.Average(); var my = y.Average();
        var cov = 0m; var sx = 0m; var sy = 0m;
        for (int i = 0; i < x.Length; i++)
        {
            var dx = x[i] - mx; var dy = y[i] - my;
            cov += dx * dy; sx += dx * dx; sy += dy * dy;
        }
        if (sx == 0m || sy == 0m) return 0m;
        return cov / (decimal)Math.Sqrt((double)(sx * sy));
    }

    private static decimal StdDev(decimal[] values)
    {
        if (values.Length <= 1) return 0m;
        var mean = values.Average();
        var var = values.Sum(v => (v - mean) * (v - mean)) / (values.Length - 1);
        return (decimal)Math.Sqrt((double)var);
    }

    private static (decimal mdd, DateTime? peak, DateTime? trough) MaxDrawdown(DailyReturn[] series)
    {
        decimal peakWealth = 1m, wealth = 1m;
        DateTime? peakDate = null, troughDate = null;
        decimal maxDD = 0m;

        foreach (var p in series.OrderBy(x => x.Date))
        {
            wealth *= (1m + p.Return);
            if (wealth > peakWealth) { peakWealth = wealth; peakDate = p.Date; }
            var dd = wealth / peakWealth - 1m; // negative or 0
            if (dd < maxDD) { maxDD = dd; troughDate = p.Date; }
        }
        return (maxDD, peakDate, troughDate);
    }
}
