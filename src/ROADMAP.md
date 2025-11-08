Absolutely, Dan. Here’s a practical, incremental plan to take your current prototype to a Clean Architecture + DDD + SOLID ASP.NET Core Web API, without losing the KISS spirit. I’ll show the target structure, key patterns, and a step‑by‑step migration path with acceptance criteria and small, copy‑pasteable code snippets.⸻

North Star: what “good” looks like

Outcomes

Domain-first: Portfolio, Accounts, Holdings, Transactions, Prices/FX, Valuations, Performance, Attribution are pure domain (no EF, no HTTP).
Use cases (Application layer) orchestrate domain logic; Web is only I/O; Infrastructure is replaceable (EF Core/SQLite now, any DB later).
SOLID everywhere:
SRP: one reason to change per class/file.
OCP/DIP: abstractions for DB, time, pricing/FX; implementation injected.
ISP/LSP: small interfaces, stable contracts.
DDD tactical:
Aggregates with invariants (e.g., Transactions mutate Holdings, cash, and flows policy).
Value Objects (Money, Currency, Symbol, DateOnly).
Domain Services (valuation, performance, attribution).
Domain Events (optional but useful: TransactionPosted, DividendReceived).
Clean boundaries: reference rules enforced by project structure.
⸻

Target solution layout (folders/projects)

src/
WebApi/ # ASP.NET Core minimal API/controllers, Swagger, DI
Application/ # Use cases (CQRS), DTOs, validators, mapping
Domain/ # Entities, Value Objects, interfaces, domain services
Infrastructure/ # EF Core, providers (price/FX), repositories, outbox (later)
Bootstrap/ # Console seed & migrations runner (optional)

tests/
UnitTests/ # Pure domain & application use-case tests
IntegrationTests/ # API + EF Core (TestServer), SQLite/Testcontainers

Rule of thumb: WebApi → Application → Domain (only inward references). Infrastructure depends on both Domain & Application but WebApi depends on Infrastructure, never the other way.⸻

Bounded contexts (start simple)

You can keep this as one bounded context initially, then split later:

Portfolio (Portfolio, Account, Holding, Transaction, CashFlow, Tag)
MarketData (Price, FxRate; providers)
Valuation (valuation snapshots)
Performance (TWR, Dietz; DailyReturn/PeriodReturn)
Benchmarking (BenchmarkDefinition, BenchmarkComponent, benchmark daily returns)
Keep one DbContext for now, with clear schemas (tables named with prefixes), then split when needed.⸻

Core Domain (DDD tactical)

Aggregates & invariants

Account (Aggregate Root)
Entities: Holding, Transaction
Invariant: Posting a Buy/Sell/Dividend updates Holdings and Cash consistently, per your policy (already implemented in your service).
Only external flows (Deposit/Withdrawal/Fee) are considered flows for TWR.
Portfolio (Aggregate Root)
Collection of Accounts; aggregation-only invariants (no cross-account mutation).
Value Objects
Money(decimal Amount, Currency Currency)
Currency(string Code)
Symbol(string Code)
Avoid primitive obsession in API contracts too.
Domain Services

ValuationService (pure)
PerformanceService (TWR, Dietz, linking; pure)
BenchmarkService (daily rebalanced; pure)
AttributionService (contribution; pure)
Keep them in Domain now; if you later need DB access inside, move parts to Application and keep computation pure.⸻

Persistence model (EF Core, SQLite dev)

Tables: Accounts, Holdings, Transactions, ValuationSnapshots, ValuationSnapshotsByAssetClass, DailyReturns, PeriodReturns, Benchmarks, BenchmarkComponents, Prices, FxRates, CashFlows.
Mappings: Keep VOs as owned types.
Example EF Core mapping (Infrastructure):

// Infrastructure/Persistence/AppDbContext.cs
public class AppDbContext : DbContext
{
public DbSet<Account> Accounts => Set<Account>();
public DbSet<Portfolio> Portfolios => Set<Portfolio>();
public DbSet<Transaction> Transactions => Set<Transaction>();
public DbSet<ValuationSnapshot> ValuationSnapshots => Set<ValuationSnapshot>();
public DbSet<DailyReturn> DailyReturns => Set<DailyReturn>();
public DbSet<BenchmarkDefinition> Benchmarks => Set<BenchmarkDefinition>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Account>(cfg =>
        {
            cfg.HasKey(x => x.Id);
            cfg.OwnsOne(x => x.Currency, cc => cc.Property(p => p.Code).HasColumnName("Currency").IsRequired());
            cfg.OwnsMany(x => x.Holdings, h =>
            {
                h.WithOwner();
                h.Property(p => p.Quantity).HasColumnType("decimal(18,4)");
                h.OwnsOne(p => p.Instrument, i =>
                {
                    i.Property(p => p.Name).HasMaxLength(200);
                    i.Property(p => p.AssetClass).HasConversion<int>();
                    i.OwnsOne(p => p.Symbol, s => s.Property(pp => pp.Code).HasColumnName("Symbol").HasMaxLength(32));
                });
            });
        });

        b.Entity<Transaction>(cfg =>
        {
            cfg.HasKey(x => x.Id);
            cfg.Property(x => x.Type).HasConversion<int>();
            cfg.OwnsOne(x => x.Amount, m =>
            {
                m.Property(p => p.Amount).HasColumnType("decimal(18,4)");
                m.OwnsOne(p => p.Currency, c => c.Property(cc => cc.Code).HasColumnName("AmountCurrency"));
            });
            cfg.OwnsOne(x => x.Costs, m =>
            {
                m.Property(p => p.Amount).HasColumnType("decimal(18,4)");
                m.OwnsOne(p => p.Currency, c => c.Property(cc => cc.Code).HasColumnName("CostsCurrency"));
            });
        });

        b.Entity<ValuationSnapshot>(cfg =>
        {
            cfg.HasKey(x => x.Id);
            cfg.Property(x => x.Period).HasConversion<int>();
            cfg.OwnsOne(x => x.Value, m =>
            {
                m.Property(p => p.Amount).HasColumnType("decimal(18,4)");
                m.OwnsOne(p => p.Currency, c => c.Property(cc => cc.Code).HasColumnName("ValueCurrency"));
            });
            cfg.OwnsOne(x => x.SecuritiesValue);
            cfg.OwnsOne(x => x.CashValue);
            cfg.OwnsOne(x => x.IncomeForDay);
        });

        b.Entity<DailyReturn>(cfg =>
        {
            cfg.HasKey(x => new { x.EntityId, x.EntityType, x.Date });
            cfg.Property(x => x.EntityType).HasConversion<int>();
            cfg.Property(x => x.Return).HasColumnType("decimal(18,8)");
        });

        b.Entity<BenchmarkDefinition>(cfg =>
        {
            cfg.HasKey(x => x.Id);
            cfg.OwnsMany(x => x.Components, c =>
            {
                c.WithOwner();
                c.OwnsOne(p => p.Instrument, i =>
                {
                    i.OwnsOne(p => p.Symbol);
                    i.Property(p => p.AssetClass).HasConversion<int>();
                });
                c.Property(p => p.Weight).HasColumnType("decimal(9,6)");
            });
        });
    }

}

Tip: Keep decimal precision explicit. Use DateOnly if you prefer; EF Core 9 supports it.⸻

Application layer (use cases, no domain leakage)

CQRS (lightweight): Command/Query classes + handlers (no need for MediatR at first; you can inject services directly).
DTOs: Request/Response models per endpoint (no domain entities over the wire).
Validation: FluentValidation (optional), or simple guards for KISS.
Mapping: Manual mapping for now (avoid AutoMapper overkill).
Example use case (Application):

public record PostTransactionCommand(Guid AccountId, TransactionType Type, string Symbol, decimal Quantity, decimal Amount, string Currency, DateTime Date, decimal? Costs);

public sealed class PostTransactionHandler
{
private readonly IAccountRepository \_accounts;
private readonly ITradeCostService \_costs;
private readonly IUnitOfWork \_uow;

    public PostTransactionHandler(IAccountRepository accounts, ITradeCostService costs, IUnitOfWork uow)
    {
        _accounts = accounts; _costs = costs; _uow = uow;
    }

    public async Task<Guid> Handle(PostTransactionCommand cmd, CancellationToken ct)
    {
        var account = await _accounts.GetByIdAsync(cmd.AccountId, ct) ?? throw new KeyNotFoundException("Account not found");

        var instrument = await _accounts.ResolveInstrumentAsync(account, cmd.Symbol, ct); // simple lookup (or from catalog)
        var tx = new Transaction
        {
            Type = cmd.Type,
            Instrument = instrument,
            Quantity = cmd.Quantity,
            Amount = new Money(cmd.Amount, Currency.From(cmd.Currency)),
            Date = cmd.Date,
            Costs = cmd.Costs.HasValue ? new Money(cmd.Costs.Value, Currency.From(cmd.Currency)) : null
        };

        // Domain service updates holdings & cash per policy
        new TransactionService().AddTransaction(account, tx, applyToCash: true);

        await _uow.SaveChangesAsync(ct);
        return tx.Id;
    }

}
⸻

Web API (ASP.NET Core)

Minimal API or Controllers—either is fine; minimal keeps KISS.
Versioning: /api/v1/... route prefix.
OpenAPI: Swashbuckle.
Example minimal endpoint (WebApi/Program.cs):

app.MapPost("/api/v1/accounts/{accountId:guid}/transactions", async (
Guid accountId, PostTransactionRequest req, PostTransactionHandler handler, CancellationToken ct) =>
{
var id = await handler.Handle(new PostTransactionCommand(
accountId, req.Type, req.Symbol, req.Quantity, req.Amount, req.Currency, req.Date, req.Costs), ct);

    return Results.Created($"/api/v1/transactions/{id}", new { id });

})
.WithName("PostTransaction")
.Produces(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest);

Keep DTOs in Application or WebApi (choose one and be consistent).⸻

Cross‑cutting policies

Error handling: global exception → ProblemDetails (400/404/409/500).
Validation: fail fast (400 with details).
Logging: structured logs (Serilog) with correlation id; minimal to start.
Observability: OpenTelemetry exporter (optional later).
Security (later): API key or Azure AD; start unauthenticated for internal dev.
Rate limiting (later): ASP.NET Core rate limiter middleware.
⸻

Testing strategy

Unit (Domain): invariants (transactions mutate holdings and cash properly), TWR math (flows neutralized), benchmark calc, attribution sums to parent.
Integration (Application + Infra): Use WebApplicationFactory + SQLite in‑memory. Validate endpoint + DB + mapping.
Contract (optional): verify JSON contracts via Snapshots.
Example unit test idea:

Given: Account with CAD cash 1000; Buy VFV for 900 + 9.99 cost → Cash=90.01; Holdings VFV += qty; TWR unaffected (no flow recorded).
⸻

Migration roadmap (incremental, 6 sprints)

Sprint 0 — Stabilize prototype

Extract your current Domain (entities, VOs, domain services) into Domain/.
Create skeletal Application/ and Infrastructure/ projects; wire DI in WebApi (empty for now).
✅ Done when solution builds; domain services pass previous console tests.
Sprint 1 — Web API shell + DTOs

Add minimal API endpoints for Portfolio, Accounts, Holdings, Transactions (POST/GET).
Introduce DTOs & simple validators.
✅ Done when basic CRUD returns JSON; simple smoke tests pass.
Sprint 2 — Persistence (EF Core + SQLite)

Add AppDbContext, mappings for Domain types (owned VOs).
Implement repositories (IAccountRepository, IPortfolioRepository, IValuationRepository) & IUnitOfWork.
Data seed: small fixture with 2 accounts, 4 instruments.
✅ Done when endpoints persist to SQLite; migrations run.
Sprint 3 — Valuation & TWR endpoints

Nightly valuation snapshots made on demand first (no background job): POST /valuation/run?from=&to=.
GET /performance/{entity}/daily (Portfolio + Account) returns daily TWR; add link=true for period link.
✅ Done when linked TWR ≈ Dietz for no-flow periods; unit tests for flow neutrality.
Sprint 4 — Benchmarks, Rolling, Calendar, Risk

Define BenchmarkDefinition (entity) + endpoints to create and attach a benchmark to a portfolio.
GET /benchmarks/{id}/daily, GET /analytics/rolling, GET /analytics/calendar, GET /analytics/risk.
✅ Done when active return = P − B; risk prints Vol, MaxDD, Sharpe.
Sprint 5 — Attribution & Asset‑class percentages

GET /attribution/{portfolioId}?scheme=AssetClass (daily/period contribution).
Include asset‑class % in valuation snapshot responses.
✅ Done when sum of contributions ≈ parent return; percentages sum to ~100%.
Sprint 6 — Hardening

Error handling, validation polish; idempotency for posting transactions; pagination/filtering.
Add caching (memory) for static MarketData; rate limiting (optional).
✅ Done when happy path + edge cases pass integration tests; docs updated.
⸻

Acceptance criteria (per theme)

Transactions: Buys/Sells/Dividends mutate holdings & cash; TWR only neutralizes Deposit/Withdrawal/Fee; costs applied correctly (net experience).
Valuations: Snapshot fields present (Total, Securities, Cash, IncomeForDay); Portfolio totals equal sum of accounts (after FX).
Performance: Daily TWR linked; Dietz available; Portfolio vs Benchmark active calculated.
Attribution: Security/AssetClass contribution sums ≈ parent return (tolerance).
APIs: OpenAPI docs; v1 routes; 95th percentile < 200ms for 1‑year daily series (in dev).
Tests: Domain unit tests (10–20 high‑value cases), 6–10 integration tests for endpoints.
⸻

A few code snippets you’ll reuse

Repository interfaces (Application or Domain):

public interface IAccountRepository
{
Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
Task AddAsync(Account account, CancellationToken ct = default);
Task UpdateAsync(Account account, CancellationToken ct = default);
Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
// Query helpers…
}

public interface IUnitOfWork
{
Task<int> SaveChangesAsync(CancellationToken ct = default);
}
DI wiring (WebApi Program.cs):

builder.Services.AddDbContext<AppDbContext>(o =>
o.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IAccountRepository, EfAccountRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

builder.Services.AddSingleton<IPriceProvider, DynamicPriceProvider>();
builder.Services.AddSingleton<IFxRateProvider, DynamicFxRateProvider>();

builder.Services.AddScoped<ValuationService>();
builder.Services.AddScoped<PerformanceService>();
builder.Services.AddScoped<BenchmarkService>();
builder.Services.AddScoped<AttributionService>();
builder.Services.AddScoped<AnalyticsService>();
builder.Services.AddScoped<TradeCostService>();
Minimal endpoint contract (WebApi/Contracts):

public record PostTransactionRequest(
TransactionType Type, string Symbol, decimal Quantity, decimal Amount, string Currency, DateTime Date, decimal? Costs);
⸻

Risks & guardrails

Over‑engineering: avoid MediatR/specifications/Outbox until you truly need them.
Time/FX gaps: dynamic providers hide missing data; prod will need real feeds & staleness policy.
Precision: define decimal scales; be consistent in currency conversion timing (EOD).
Consistency: one policy doc: flows vs trades, net/gross basis, benchmark rebalancing convention.
⸻

What I can hand you next

A repo skeleton with these projects and a minimal working set (controllers/minimal APIs + EF Core mappings + 3–4 endpoints wired).
A scripted migration from your console prototype into the layered solution, preserving your domain code unchanged.
A developer cookbook (README) describing flows to post trades, run valuations, query TWR/benchmark/rolling/risk, and print cost reports.
⸻

If this plan fits, I’ll spin up Sprint 0/1 scaffolding for you: projects, DI, a couple of endpoints (post transaction, get portfolio value), and EF Core mappings for your VOs—ready to run with SQLite.
