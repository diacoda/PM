using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Application.Services
{
    public class ValuationService : IValuationService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IValuationRepository _valuationRepository;
        private readonly IPricingService _pricingService;

        public ValuationService(
            IPortfolioRepository portfolioRepository,
            IAccountRepository accountRepository,
            IValuationRepository valuationRepository,
            IPricingService pricingService)
        {
            _portfolioRepository = portfolioRepository;
            _accountRepository = accountRepository;
            _valuationRepository = valuationRepository;
            _pricingService = pricingService;
        }

        // ---------------- SNAPSHOT GENERATION ----------------

        public async Task<Valuation> GeneratePortfolioValuation(
            int portfolioId,
            DateOnly date,
            Currency reportingCurrency,
            CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepository.GetByIdWithIncludesAsync(
                portfolioId,
                new[] { IncludeOption.Accounts, IncludeOption.Holdings },
                ct);

            if (portfolio is null)
                throw new ArgumentException($"Portfolio {portfolioId} not found.");

            var total = await _pricingService.CalculatePortfolioValueAsync(portfolio, date, reportingCurrency, ct);

            decimal cashAmt = 0m;
            foreach (var account in portfolio.Accounts)
                cashAmt += await SumAccountCashAsync(account, date, reportingCurrency, ct);

            var cash = new Money(cashAmt, reportingCurrency);
            var secs = new Money(total.Amount - cashAmt, reportingCurrency);

            decimal netIncome = 0m;
            foreach (var account in portfolio.Accounts)
                netIncome += SumAccountNetIncomeForDay(account, date, reportingCurrency);

            return new Valuation(total, secs, cash, netIncome == 0m ? null : new Money(netIncome, reportingCurrency), reportingCurrency, AssetClass.None, 0m);
        }

        public async Task<Valuation> GenerateAccountValuation(
            int portfolioId,
            int accountId,
            DateOnly date,
            Currency reportingCurrency,
            CancellationToken ct = default)
        {
            var account = await _accountRepository.GetByIdWithIncludesAsync(
                accountId,
                new[] { IncludeOption.Holdings },
                ct);

            if (account is null)
                throw new ArgumentException($"Account {accountId} not found.");

            var total = await _pricingService.CalculateAccountValueAsync(account, date, reportingCurrency, ct);

            var cashAmt = await SumAccountCashAsync(account, date, reportingCurrency, ct);
            var cash = new Money(cashAmt, reportingCurrency);
            var secs = new Money(total.Amount - cashAmt, reportingCurrency);

            var netIncome = SumAccountNetIncomeForDay(account, date, reportingCurrency);
            
            return new Valuation(total, secs, cash, netIncome == 0m ? null : new Money(netIncome, reportingCurrency), reportingCurrency, AssetClass.None, 0m);
        }

        public async Task<IEnumerable<Valuation>> GeneratePortfolioAssetClassValuation(
            int portfolioId,
            DateOnly date,
            Currency reportingCurrency,
            CancellationToken ct = default)
        {
            var portfolio = await _portfolioRepository.GetByIdWithIncludesAsync(
                portfolioId,
                new[] { IncludeOption.Accounts, IncludeOption.Holdings },
                ct);

            if (portfolio is null)
                throw new ArgumentException($"Portfolio {portfolioId} not found.");

            var totalMoney = await _pricingService.CalculatePortfolioValueAsync(portfolio, date, reportingCurrency, ct);
            var total = totalMoney.Amount;
            if (total <= 0m) return Enumerable.Empty<Valuation>();

            var byClass = await AggregateByAssetClassAsync(portfolio, date, reportingCurrency, ct);
            return byClass
                .Select(kvp => new Valuation(
                    kvp.Value,
                    new Money(0m, reportingCurrency),
                    new Money(0m, reportingCurrency),
                    new Money(0m, reportingCurrency),
                    reportingCurrency,
                    kvp.Key,
                    kvp.Value.Amount / total
                ))
                .ToList();
        }

        public async Task<IEnumerable<Valuation>> GenerateAccountAssetClassValuation(
            int portfolioId,
            int accountId,
            DateOnly date,
            Currency reportingCurrency,
            CancellationToken ct = default)
        {
            var account = await _accountRepository.GetByIdWithIncludesAsync(
                accountId,
                new[] { IncludeOption.Holdings },
                ct);

            if (account is null)
                throw new ArgumentException($"Account {accountId} not found.");

            var totalMoney = await _pricingService.CalculateAccountValueAsync(account, date, reportingCurrency, ct);
            var total = totalMoney.Amount;
            if (total <= 0m) return Enumerable.Empty<Valuation>();

            var byClass = await AggregateByAssetClassAsync(account, date, reportingCurrency, ct);
            return byClass.Select(kvp => new Valuation(
                kvp.Value,
                new Money(0m, reportingCurrency),
                new Money(0m, reportingCurrency),
                new Money(0m, reportingCurrency),
                reportingCurrency,
                kvp.Key,
                kvp.Value.Amount / total))
            .ToList();
        }

        // ---------------- STORAGE ----------------
        public async Task StoreEstateValuation(Valuation valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            var record = new ValuationSnapshot
            {
                Id = $"{EntityKind.Estate.ToString()}:estate:{period.ToString()}:{date.ToString()}:{ValuationType.Standard.ToString()}:{AssetClass.None.ToString()}",
                Kind = EntityKind.Estate,
                Date = date,
                Period = period,
                Type = ValuationType.Standard,
                ReportingCurrency = valuation.ReportingCurrency,
                AssetClass = AssetClass.None,

                Value = valuation.TotalValue,
                SecuritiesValue = valuation.SecuritiesValue,
                CashValue = valuation.CashValue,
                IncomeForDay = valuation.IncomeForDay
            };
            await _valuationRepository.SaveAsync(record, ct);
        }

        public async Task StoreEstateAssetClassValuation(IEnumerable<Valuation> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            foreach (var valuation in valuations)
            {
                var record = new ValuationSnapshot
                {
                    Id = $"{EntityKind.Estate.ToString()}:estate:{period.ToString()}:{date.ToString()}:{ValuationType.AssetClass.ToString()}:{valuation.AssetClass.ToString()}",
                    Kind = EntityKind.Estate,
                    Period = period,
                    Date = date,
                    Type = ValuationType.AssetClass,

                    Value = valuation.TotalValue,
                    SecuritiesValue = valuation.SecuritiesValue,
                    CashValue = valuation.CashValue,
                    IncomeForDay = valuation.IncomeForDay,
                    AssetClass = valuation.AssetClass,
                    Percentage = valuation.Percentage
                };
                await _valuationRepository.SaveAsync(record, ct);
            }
        }

        public async Task StoreOwnerValuation(string owner, Valuation valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            var record = new ValuationSnapshot
            {
                Id = $"{EntityKind.Owner.ToString()}:{owner}:{period.ToString()}:{date.ToString()}:{ValuationType.Standard.ToString()}:{AssetClass.None.ToString()}",
                Kind = EntityKind.Owner,
                Owner = owner,
                Date = date,
                Period = period,
                Type = ValuationType.Standard,
                ReportingCurrency = valuation.ReportingCurrency,
                AssetClass = AssetClass.None,

                Value = valuation.TotalValue,
                SecuritiesValue = valuation.SecuritiesValue,
                CashValue = valuation.CashValue,
                IncomeForDay = valuation.IncomeForDay
            };
            await _valuationRepository.SaveAsync(record, ct);
        }

        public async Task StoreOwnerAssetClassValuation(string owner, IEnumerable<Valuation> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            foreach (var valuation in valuations)
            {
                var record = new ValuationSnapshot
                {
                    Id = $"{EntityKind.Owner.ToString()}:{owner}:{period.ToString()}:{date.ToString()}:{ValuationType.AssetClass.ToString()}:{valuation.AssetClass.ToString()}",
                    Kind = EntityKind.Owner,
                    Period = period,
                    Owner = owner,
                    Date = date,
                    Type = ValuationType.AssetClass,
                    
                    Value = valuation.TotalValue,
                    SecuritiesValue = valuation.SecuritiesValue,
                    CashValue = valuation.CashValue,
                    IncomeForDay = valuation.IncomeForDay,
                    AssetClass = valuation.AssetClass,
                    Percentage = valuation.Percentage
                };
                await _valuationRepository.SaveAsync(record, ct);
            }
        }

        public async Task StorePortfolioValuation(int portfolioId, Valuation valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            var record = new ValuationSnapshot
            {
                Id = $"{EntityKind.Portfolio.ToString()}:{portfolioId}:{period.ToString()}:{date.ToString()}:{ValuationType.Standard.ToString()}:{AssetClass.None.ToString()}",
                Kind = EntityKind.Portfolio,
                Date = date,
                Period = period,
                PortfolioId = portfolioId,
                Type = ValuationType.Standard,
                ReportingCurrency = valuation.ReportingCurrency,
                AssetClass = AssetClass.None,

                Value = valuation.TotalValue,
                SecuritiesValue = valuation.SecuritiesValue,
                CashValue = valuation.CashValue,
                IncomeForDay = valuation.IncomeForDay
            };

            await _valuationRepository.SaveAsync(record, ct);
        }

        public async Task StoreAccountValuation(int portfolioId, int accountId, Valuation valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            var record = new ValuationSnapshot
            {
                Id = $"{EntityKind.Account.ToString()}:{accountId}:{period.ToString()}:{date.ToString()}:{ValuationType.Standard.ToString()}:{AssetClass.None.ToString()}",
                Kind = EntityKind.Account,
                Period = period,
                PortfolioId = portfolioId,
                AccountId = accountId,
                Date = date,
                Type = ValuationType.Standard,
                AssetClass = AssetClass.None,

                Value = valuation.TotalValue,
                SecuritiesValue = valuation.SecuritiesValue,
                CashValue = valuation.CashValue,
                IncomeForDay = valuation.IncomeForDay
            };
            await _valuationRepository.SaveAsync(record, ct);
        }

        public async Task StorePortfolioAssetClassValuation(int portfolioId, IEnumerable<Valuation> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            foreach (var v in valuations)
            {
                var record = new ValuationSnapshot
                {
                    Id = $"{EntityKind.Portfolio.ToString()}:{portfolioId}:{period.ToString()}:{date.ToString()}:{ValuationType.AssetClass.ToString()}:{v.AssetClass.ToString()}",
                    Kind = EntityKind.Portfolio,
                    Period = period,
                    PortfolioId = portfolioId,
                    Date = date,
                    Type = ValuationType.AssetClass,

                    Value = v.TotalValue,
                    SecuritiesValue = v.SecuritiesValue,
                    CashValue = v.CashValue,
                    IncomeForDay = v.IncomeForDay,
                    AssetClass = v.AssetClass,
                    Percentage = v.Percentage
                };
                await _valuationRepository.SaveAsync(record, ct);
            }
        }

        public async Task StoreAccountAssetClassValuation(int portfolioId, int accountId, IEnumerable<Valuation> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            foreach (var v in valuations)
            {
                var record = new ValuationSnapshot
                {
                    Id = $"{EntityKind.Account.ToString()}:{accountId}:{period.ToString()}:{date.ToString()}:{ValuationType.AssetClass.ToString()}:{v.AssetClass.ToString()}",
                    Kind = EntityKind.Account,
                    Period = period,
                    PortfolioId = portfolioId,
                    AccountId = accountId,
                    Date = date,
                    Type = ValuationType.AssetClass,

                    Value = v.TotalValue,
                    SecuritiesValue = v.SecuritiesValue,
                    CashValue = v.CashValue,
                    IncomeForDay = v.IncomeForDay,
                    AssetClass = v.AssetClass,
                    Percentage = v.Percentage
                };
                await _valuationRepository.SaveAsync(record, ct);
            }
        }

        // ---------------- PRIVATE HELPERS ----------------

        private async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Account account, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
        {
            var result = new Dictionary<AssetClass, Money>();
            foreach (var holding in account.Holdings)
            {
                var value = await _pricingService.CalculateHoldingValueAsync(holding, date, reportingCurrency, ct);
                var cls = holding.Asset.AssetClass;
                if (result.TryGetValue(cls, out var existing))
                    result[cls] = new Money(existing.Amount + value.Amount, reportingCurrency);
                else
                    result[cls] = value;
            }
            return result;
        }

        private async Task<Dictionary<AssetClass, Money>> AggregateByAssetClassAsync(Portfolio portfolio, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
        {
            var result = new Dictionary<AssetClass, Money>();
            foreach (var account in portfolio.Accounts)
            {
                var accAgg = await AggregateByAssetClassAsync(account, date, reportingCurrency, ct);
                foreach (var kvp in accAgg)
                {
                    if (result.TryGetValue(kvp.Key, out var existing))
                        result[kvp.Key] = new Money(existing.Amount + kvp.Value.Amount, reportingCurrency);
                    else
                        result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        private async Task<decimal> SumAccountCashAsync(Account account, DateOnly date, Currency reportingCurrency, CancellationToken ct = default)
        {
            decimal cash = 0m;
            foreach (var h in account.Holdings.Where(h => h.Asset.AssetClass == AssetClass.Cash))
            {
                var val = await _pricingService.CalculateHoldingValueAsync(h, date, reportingCurrency, ct);
                cash += val.Amount;
            }
            return cash;
        }

        private decimal SumAccountNetIncomeForDay(Account account, DateOnly date, Currency reportingCurrency)
        {
            var net = account.Transactions
                .Where(t => t.Type.IsIncome() && t.Date == date)
                .Where(t => t.Amount.Currency == reportingCurrency)
                .Sum(t => (t.Amount.Amount - (t.Costs?.Amount ?? 0m)));
            return net;
        }

        // ---------------- READ ----------------

        public async Task<ValuationSnapshot?> GetLatestAsync(
            EntityKind kind,
            int entityId,
            Currency currency,
            ValuationPeriod? period = null,
            bool includeAssetClass = false,
            CancellationToken ct = default)
        {
            return await _valuationRepository.GetLatestAsync(kind, entityId, currency, period, includeAssetClass, ct);
        }

        public async Task<IEnumerable<ValuationSnapshot>> GetHistoryAsync(
            EntityKind kind,
            int entityId,
            DateOnly start,
            DateOnly end,
            Currency currency,
            ValuationPeriod? period = null,
            CancellationToken ct = default)
        {
            return await _valuationRepository.GetRangeAsync(kind, entityId, start, end, currency, period, null, ct);
        }

        public async Task<IEnumerable<ValuationSnapshot>> GetAsOfDateAsync(
            EntityKind kind,
            DateOnly date,
            Currency currency,
            ValuationPeriod? period = null,
            CancellationToken ct = default)
        {
            return await _valuationRepository.GetAsOfDateAsync(kind, date, currency, period, ct);
        }
    }
}
