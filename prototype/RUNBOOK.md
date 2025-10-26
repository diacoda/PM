# RUNBOOK — How to execute and verify this prototype

> **Prereqs**: .NET 9 SDK

## 1) Build & run

```bash
dotnet build
dotnet run --project ./demo.csproj
```

- When prompted, enter a reporting currency (e.g., `CAD`).
- The app seeds:
  - Portfolio “Person1”
  - Accounts: RRSP (CAD), TFSA (CAD)
  - Instruments: VFV.TO, VCE.TO, VOO, USBOND, CASH.CAD, CASH.USD
  - Starting holdings, a mix of external flows and internal events (trades/dividend)
  - Dynamic daily **Prices** and **FX** (no external data).

## 2) What appears (and how to read it)

1. **Stored Portfolio Valuations (TOTAL)**  
   Daily totals in your reporting currency for the selected window.

2. **[Snapshot components]**  
   For the end date: **Total**, **Securities**, **Cash**, **IncomeForDay**.  
   Use this to confirm **Total ≈ Securities + Cash**.

3. **[SAVED] Portfolio Asset‑Class %**  
   Asset‑class breakdown with weights (percentages saved with the snapshot).

4. **Portfolio Daily TWR** + **Linked Portfolio Return**  
   TWR neutralizes external flows (deposits/withdrawals/fees).  
   Linked return is the period performance figure.

5. **Account Daily TWR** + linked return for each account.

6. **Portfolio Period Return (Modified Dietz)**  
   Sanity check against TWR (close when flows are small).

7. **Contribution by Security / AssetClass**  
   Shows start‑weight × return and contribution totals.

8. **Benchmark (75/25 CAD) & Daily Returns**  
   Benchmark recipe is printed; daily returns (last 7 days).  
   Then **Linked** P, **Linked** B, and **Active** (difference).

9. **Rolling Returns (as of end)**  
   Portfolio, Benchmark, and Active for 1M/3M/6M/YTD/1Y/3Y/SI.

10. **Calendar Monthly Returns**  
    Two lines per year: P and B; also prints YTD.

11. **Risk Card**  
    Vol (annualized), Max Drawdown (with peak/trough), Sharpe (Rf≈0), Hit rate, Correlation to benchmark.

12. **Transaction Costs** (Portfolio + each Account)  
    Totals by currency and cost rates, plus top securities by cost.

13. **Updated Holdings Summary** and **Transaction History**  
    Human‑readable dump for inspection.

14. **Asset Class Aggregation (end date)**  
    Total value by asset class in reporting currency.

## 3) Verify key quality checks

- **Total = Securities + Cash (per snapshot)**: check the printed component lines.
- **Portfolio = Σ Accounts (same date/currency)**: sums should match closely.
- **Active = P − B (linked)**: the printed line shows the arithmetic difference.
- **Contributions sum to parent TWR**: eyeball; minor rounding differences are expected.
- **Asset‑class % sum to ~100%**: the % block at end date should add up.

## 4) Change inputs (quick experiments)

- **Reporting currency**: rerun and input `USD` or `EUR`.
- **Trade costs**: tweak `TradeCostService` rules (fixed/pct/min).
- **Benchmark weights**: edit the 75/25 recipe (e.g., 60/40).
- **Window length**: change `start`/`end` in `Program.cs` to see longer histories.

## 5) Where to add new activity

In `Program.cs`:

- **External flows** (affect TWR): `Deposit`, `Withdraw`, `Fee`  
  → also recorded in `CashFlowService`.
- **Internal events** (no TWR flow): `Buy`, `Sell`, `Dividend`  
  → mutate holdings and cash; carry `Costs` as needed.

## 6) Common pitfalls

- **Posting Buy/Sell/Dividend as flows** will distort TWR. Keep them **internal**.
- **Zero start value** on a day sets TWR to 0 for that day (defensive guard).
- **Costless trades**: if `Costs` are omitted or zero, cost reports won’t show entries.

## 7) Next steps (when you’re ready)

- Promote this domain into a **Clean Architecture Web API**:
  - Use Cases for posting transactions, running valuations, returning time series.
  - SQLite/EF Core Infrastructure for persistence.
  - Swagger for discoverability; a small React/Blazor later if needed.
