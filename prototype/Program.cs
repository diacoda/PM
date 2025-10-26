using model.Domain.Entities;
using model.Domain.Values;
using model.Interfaces;
using model.Providers;
using model.Repositories;
using model.Services;

// ---------------------------------------------------------------
// Demo console app (KISS, model-only) with:
// - Dynamic daily Prices/FX
// - Trades (Buy/Sell/Dividend) that mutate holdings & cash (with costs)
// - External flows (Deposit/Withdrawal/Fee) that TWR neutralizes
// - Daily valuations (total + asset-class % + snapshot components)
// - Daily TWR + linked; Modified Dietz
// - Contribution (Security + AssetClass)
// - Benchmark (75/25 CAD) with rolling & calendar returns
// - Risk Card
// - Transaction cost reporting (portfolio + accounts)
// ---------------------------------------------------------------

/* 0) In-memory setup */
var portfolioService = new PortfolioService();
var accountService = new AccountService();
var holdingService = new HoldingService();
var transactionService = new TransactionService();
var cashFlowService = new CashFlowService();
var valuationRepository = new ValuationRepository();

/* 1) Portfolio & Accounts */
var portfolio = portfolioService.Create("Person1");
var rrsp = accountService.Create("RRSP", Currency.From("CAD"), FinancialInstitutions.TD);
var tfsa = accountService.Create("TFSA", Currency.From("CAD"), FinancialInstitutions.WS);
accountService.AddAccountToPortfolio(portfolio, rrsp);
accountService.AddAccountToPortfolio(portfolio, tfsa);

IEnumerable<Account> accounts = accountService.ListAccounts(portfolio);
Console.WriteLine("Accounts:");
foreach (Account account in accounts)
{
    Console.WriteLine(account);
}

/* 2) Instruments & initial holdings (quantities will change when we trade) */
var vfv = new Instrument(Symbol.From("VFV.TO"), "Vanguard S&P 500 (CAD)", AssetClass.Equity);
var vce = new Instrument(Symbol.From("VCE.TO"), "Vanguard Canadian Equity", AssetClass.Equity);
var voo = new Instrument(Symbol.From("VOO"), "Vanguard S&P 500 (USD)", AssetClass.Equity);
var usbond = new Instrument(Symbol.From("USBOND"), "US Treasury Bond", AssetClass.FixedIncome);
var cadCash = new Instrument(Symbol.From("CASH.CAD"), "Canadian Dollar", AssetClass.Cash);
var usdCash = new Instrument(Symbol.From("CASH.USD"), "US Dollar", AssetClass.Cash);

// Seed some starting positions
holdingService.AddHolding(rrsp, vfv, 10);
holdingService.AddHolding(rrsp, voo, 5);
holdingService.AddHolding(tfsa, vce, 20);
holdingService.AddHolding(tfsa, usbond, 2);

/* 3) Dynamic providers & core services */
IPriceProvider priceProvider = new DynamicPriceProvider();
IFxRateProvider fxProvider = new DynamicFxRateProvider();

var valuationService = new ValuationService(priceProvider, fxProvider);
var reportingService = new ReportingService(valuationService);
var valuationManager = new ValuationManager(valuationService, valuationRepository);
var performanceService = new PerformanceService(valuationService, cashFlowService);
var attributionService = new AttributionService(valuationService);
var benchmarkService = new BenchmarkService(valuationService);
var analyticsService = new AnalyticsService();

/* Transaction costs calculator */
var costService = new TradeCostService(
    buySellRules: new Dictionary<string, TradeCostService.BuySellRule>
    {
        // Fixed + % per trade; optional minimum; currency-specific (example rules)
        ["CAD"] = new(Fixed: 9.99m, Pct: 0.0005m, Min: 9.99m), // $9.99 + 0.05%
        ["USD"] = new(Fixed: 4.99m, Pct: 0.0004m, Min: 4.99m)
    },
    dividendWithholdingPct: new Dictionary<string, decimal>
    {
        ["USD"] = 0.15m // 15% withholding example
    }
);

/* 4) Reporting currency & analysis window */
Console.Write("\nEnter reporting currency (e.g., CAD, USD, EUR): ");
var reportingCurrencyInput = "CAD"; //Console.ReadLine()?.ToUpper() ?? "CAD";
var CCY = Currency.From(reportingCurrencyInput);

// Use ~3 weeks so rolling & calendar are more interesting in a single run
var start = DateTime.Today.AddDays(-20).Date;
var end = DateTime.Today.Date;

IAccountManager accountManager = new AccountManager(transactionService, costService, cashFlowService);
/* 5) Helpers: external flows (record in CashFlowService) vs internal events (no cash flows) */
// External flows — affect CASH and are recorded in CashFlowService so TWR neutralizes them
// Deposit Withdraw Fee


// Internal events — affect cash/quantities but NOT CashFlowService (keeps TWR clean)
// Buy Sell Dividend

/* 6) Populate flows & trades across the window */
accountManager.Deposit(rrsp, 1500m, "CAD", start.AddDays(1), "RRSP CAD contribution");
accountManager.Deposit(rrsp, 1200m, "USD", start.AddDays(3), "RRSP USD contribution");
accountManager.Fee(rrsp, 15m, "CAD", start.AddDays(6), "RRSP platform fee");
accountManager.Withdraw(rrsp, 200m, "CAD", start.AddDays(9), "RRSP withdrawal");
accountManager.Deposit(rrsp, 500m, "CAD", start.AddDays(14), "RRSP top-up");

accountManager.Deposit(tfsa, 1800m, "CAD", start.AddDays(2), "TFSA CAD contribution");
accountManager.Fee(tfsa, 10m, "CAD", start.AddDays(11), "TFSA account fee");
accountManager.Deposit(tfsa, 300m, "CAD", start.AddDays(17), "TFSA small contribution");

// Real trades (update holdings & cash) + a dividend
accountManager.Buy(rrsp, vfv, 2m, 900m, "CAD", start.AddDays(4), "Add VFV");
accountManager.Sell(rrsp, voo, 1m, 420m, "USD", start.AddDays(12), "Trim VOO");
accountManager.Buy(tfsa, vce, 3m, 600m, "CAD", start.AddDays(5), "Add VCE");
accountManager.Dividend(tfsa, vce, 25m, "CAD", start.AddDays(8), "VCE distribution");

/* 7) Valuations (TOTAL) with clean components saved into snapshots */
valuationManager.GenerateAndStorePortfolioValuations(portfolio, start, end, CCY, ValuationPeriod.Daily);
foreach (var acct in portfolio.Accounts)
    valuationManager.GenerateAndStoreAccountValuations(acct, start, end, CCY, ValuationPeriod.Daily);

// Show TOTAL portfolio valuations
Console.WriteLine($"\nStored Portfolio Valuations (TOTAL) [{start:yyyy-MM-dd} .. {end:yyyy-MM-dd}] ({CCY}):");
foreach (var v in valuationRepository.GetByPortfolio(portfolio.Id, ValuationPeriod.Daily).OrderBy(v => v.Date))
    Console.WriteLine($"{v.Date:yyyy-MM-dd}  {v.Value.Amount:0.##} {v.Value.Currency}");

// Show snapshot components for END date
var endSnap = valuationRepository.GetByPortfolio(portfolio.Id, ValuationPeriod.Daily)
               .Where(v => v.Date.Date == end.Date && v.AssetClass == null)
               .OrderByDescending(v => v.Date).FirstOrDefault();
if (endSnap != null)
{
    Console.WriteLine($"\n[Snapshot components @ {end:yyyy-MM-dd}] ({CCY})");
    Console.WriteLine($"  Total     : {endSnap.Value.Amount:0.##}");
    Console.WriteLine($"  Securities: {endSnap.SecuritiesValue?.Amount ?? 0:0.##}");
    Console.WriteLine($"  Cash      : {endSnap.CashValue?.Amount ?? 0:0.##}");
    Console.WriteLine($"  Income(day): {endSnap.IncomeForDay?.Amount ?? 0:0.##}");
}

/* 8) Valuations BY ASSET CLASS (with % saved) */
valuationManager.GenerateAndStorePortfolioValuationsByAssetClass(portfolio, start, end, CCY, ValuationPeriod.Daily);
foreach (var acct in portfolio.Accounts)
    valuationManager.GenerateAndStoreAccountValuationsByAssetClass(acct, start, end, CCY, ValuationPeriod.Daily);

// Saved asset-class % for END date
Console.WriteLine($"\n[SAVED] Portfolio Asset-Class % on {end:yyyy-MM-dd} ({CCY}):");
var pClassEnd = valuationRepository.GetPortfolioAssetClassOnDate(portfolio.Id, end, ValuationPeriod.Daily);
foreach (var s in pClassEnd)
    Console.WriteLine($"  {s.AssetClass,-12} : {(s.Percentage.GetValueOrDefault() * 100m):0.00}% ({s.Value.Amount:0.##} {s.Value.Currency})");

/* 9) Daily TWR (Portfolio + Accounts) & linked */
Console.WriteLine($"\nPortfolio Daily TWR [{start:yyyy-MM-dd} .. {end:yyyy-MM-dd}] ({CCY}):");
var pDaily = performanceService.GetPortfolioDailyTwr(portfolio, start, end, CCY).OrderBy(x => x.Date).ToList();
foreach (var dr in pDaily)
{
    Console.WriteLine($"  {dr.Date:yyyy-MM-dd} : {dr.Return:P4}");
}
var pLinked = performanceService.Link(pDaily.Select(x => x.Return));
Console.WriteLine($"Linked Portfolio Return: {pLinked:P4}");

foreach (var acct in portfolio.Accounts)
{
    Console.WriteLine($"\nAccount Daily TWR — {acct.Name} [{start:yyyy-MM-dd} .. {end:yyyy-MM-dd}] ({CCY}):");
    var aDaily = performanceService.GetAccountDailyTwr(acct, start, end, CCY).OrderBy(x => x.Date).ToList();
    foreach (var dr in aDaily)
    {
        Console.WriteLine($"  {dr.Date:yyyy-MM-dd} : {dr.Return:P4}");
    }
    var aLinked = performanceService.Link(aDaily.Select(x => x.Return));
    Console.WriteLine($"Linked Return ({acct.Name}): {aLinked:P4}");
}

/* 10) Period (Modified Dietz) — compare vs linked TWR */
var pDietz = performanceService.GetPortfolioReturn(portfolio, start, end, CCY);
Console.WriteLine($"\nPortfolio Period Return (Modified Dietz) [{start:yyyy-MM-dd} .. {end:yyyy-MM-dd}]: {pDietz.Return:P4}");

/* 11) Contribution (Security & AssetClass) over the period */
Console.WriteLine($"\nContribution by Security (Portfolio) [{start:yyyy-MM-dd} .. {end:yyyy-MM-dd}] ({CCY}):");
var contribSec = attributionService.ContributionBySecurity(portfolio, start, end, CCY).OrderByDescending(c => c.Contribution);
foreach (var c in contribSec)
{
    Console.WriteLine($"  {c.Level,-10} {c.Key,-10} w0={c.StartWeight:P2}  r={c.Return:P3}  ctrb={c.Contribution:P3}");
}

Console.WriteLine($"\nContribution by AssetClass (Portfolio) [{start:yyyy-MM-dd} .. {end:yyyy-MM-dd}] ({CCY}):");
var contribAc = attributionService.ContributionByAssetClass(portfolio, start, end, CCY).OrderByDescending(c => c.Contribution);
foreach (var c in contribAc)
{
    Console.WriteLine($"  {c.Level,-10} {c.Key,-10} w0={c.StartWeight:P2}  r={c.Return:P3}  ctrb={c.Contribution:P3}");
}

/* 12) Benchmark (75/25 CAD — daily rebalanced) */
var bench = new BenchmarkDefinition(
    name: "75/25 CAD (Daily rebalanced)",
    reportingCurrency: CCY,
    components: new List<BenchmarkComponent>
    {
        new BenchmarkComponent(voo,    0.50m), // US Equity
        new BenchmarkComponent(vce,    0.25m), // CA Equity
        new BenchmarkComponent(usbond, 0.25m)  // Bonds
    }
);

var bDaily = benchmarkService.GetDailyBenchmarkReturns(bench, start, end).ToArray();

Console.WriteLine($"\nBenchmark: {bench.Name} | Policy: {bench.RebalancePolicy}");
foreach (var c in bench.Components)
    Console.WriteLine($"  - {c.Instrument.Name,-24} ({c.Instrument.Symbol})  w={c.Weight:P0}");

Console.WriteLine("\nBenchmark Daily Returns (last 7 days):");
foreach (var dr in bDaily.TakeLast(7))
    Console.WriteLine($"  {dr.Date:yyyy-MM-dd} : {dr.Return:P4}");

// Linked Portfolio vs Benchmark vs Active
var linkP = analyticsService.Link(pDaily.Select(x => x.Return));
var linkB = analyticsService.Link(bDaily.Select(x => x.Return));
var linkActive = linkP - linkB;

Console.WriteLine($"\nLinked Returns [{start:yyyy-MM-dd} .. {end:yyyy-MM-dd}] ({CCY}):");
Console.WriteLine($"  Portfolio : {linkP:P4}");
Console.WriteLine($"  Benchmark : {linkB:P4}");
Console.WriteLine($"  Active    : {linkActive:P4}");

/* 13) Rolling returns (as of 'end') */
var rollP = analyticsService.ComputeRolling(pDaily.ToArray(), end, start);
var rollB = analyticsService.ComputeRolling(bDaily, end, start);
decimal Active(decimal rp, decimal rb) => rp - rb;

Console.WriteLine($"\nRolling Returns (as of {end:yyyy-MM-dd}) — linked %");
void PrintRoll(string label, RollingReturnSet r)
{
    Console.WriteLine($"{label,-10}  1M:{r.R_1M,7:P2}  3M:{r.R_3M,7:P2}  6M:{r.R_6M,7:P2}  YTD:{r.R_YTD,7:P2}  1Y:{r.R_1Y,7:P2}  3Y:{r.R_3Y,7:P2}  SI:{r.R_SI,7:P2}");
}
PrintRoll("Portfolio", rollP);
PrintRoll("Benchmark", rollB);
Console.WriteLine($"{"Active",-10}  1M:{Active(rollP.R_1M, rollB.R_1M),7:P2}  3M:{Active(rollP.R_3M, rollB.R_3M),7:P2}  6M:{Active(rollP.R_6M, rollB.R_6M),7:P2}  " +
                  $"YTD:{Active(rollP.R_YTD, rollB.R_YTD),7:P2}  1Y:{Active(rollP.R_1Y, rollB.R_1Y),7:P2}  3Y:{Active(rollP.R_3Y, rollB.R_3Y),7:P2}  SI:{Active(rollP.R_SI, rollB.R_SI),7:P2}");

/* 14) Calendar monthly returns (Portfolio vs Benchmark) */
Console.WriteLine($"\nCalendar Monthly Returns (%), Portfolio vs Benchmark (non-overlapping)");
var calP = analyticsService.CalendarMonthlyReturns(pDaily.ToArray());
var calB = analyticsService.CalendarMonthlyReturns(bDaily);

var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
var years = calP.Keys.Select(k => k.Year).Union(calB.Keys.Select(k => k.Year)).Distinct().OrderBy(y => y).ToArray();

Console.WriteLine($"{"",8}{string.Join("", months.Select(m => $"{m,7}"))}{"   YTD",7}");
foreach (var y in years)
{
    decimal Ytd(Dictionary<(int Year, int Month), decimal> map) =>
        map.Where(kv => kv.Key.Year == y).OrderBy(kv => kv.Key.Month).Select(kv => kv.Value)
           .Aggregate(1m, (acc, r) => acc * (1m + r)) - 1m;

    Console.Write($"{"P " + y,8}");
    for (int m = 1; m <= 12; m++)
    {
        calP.TryGetValue((y, m), out var r);
        Console.Write($"{(r == 0 ? 0 : r),7:P1}");
    }
    Console.Write($"{Ytd(calP.Where(k => k.Key.Year == y).ToDictionary(k => k.Key, v => v.Value)),7:P1}");
    Console.WriteLine();

    Console.Write($"{"B " + y,8}");
    for (int m = 1; m <= 12; m++)
    {
        calB.TryGetValue((y, m), out var r);
        Console.Write($"{(r == 0 ? 0 : r),7:P1}");
    }
    Console.Write($"{Ytd(calB.Where(k => k.Key.Year == y).ToDictionary(k => k.Key, v => v.Value)),7:P1}");
    Console.WriteLine();
}

/* 15) Risk card */
var riskP = analyticsService.ComputeRisk(pDaily.ToArray());
var riskPCorrBench = analyticsService.ComputeRisk(pDaily.ToArray(), bDaily);

Console.WriteLine("\nRisk Card (daily, period shown):");
Console.WriteLine($"  Vol (ann): {riskP.VolAnnual:P2}   Max DD: {riskP.MaxDrawdown:P2} (peak {riskP.PeakDate:yyyy-MM-dd}, trough {riskP.TroughDate:yyyy-MM-dd})");
Console.WriteLine($"  Sharpe: {riskP.Sharpe:0.00}   Hit Rate (up-days): {riskP.HitRateDaily:P0}");
Console.WriteLine($"  Corr to Benchmark: {riskPCorrBench.CorrelationToBenchmark.GetValueOrDefault():0.00}");

/* 16) Transaction cost reporting (portfolio + accounts) */
reportingService.PrintTransactionCostReport(portfolio, start, end);
foreach (var acct in portfolio.Accounts)
    reportingService.PrintTransactionCostReport(acct, start, end);

/* 17) Friendly summaries */
Console.WriteLine("\nUpdated Holdings Summary:");
reportingService.PrintHoldingsSummary(rrsp);
reportingService.PrintHoldingsSummary(tfsa);

Console.WriteLine("\nTransaction History (window):");
reportingService.PrintTransactionHistory(rrsp, start, end);
reportingService.PrintTransactionHistory(tfsa, start, end);

Console.WriteLine($"\nAsset Class Aggregation ({CCY}) on {end:yyyy-MM-dd}:");
var assetClassTotals = reportingService.AggregateByAssetClass(portfolio, end, CCY);
foreach (var kvp in assetClassTotals.OrderByDescending(k => k.Value.Amount))
    Console.WriteLine($"{kvp.Key}: {kvp.Value.Amount:0.##} {kvp.Value.Currency}");

Console.WriteLine("\nDone.");
