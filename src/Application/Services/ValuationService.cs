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

        public async Task<ValuationRecord> GeneratePortfolioValuationSnapshot(
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

            decimal divNet = 0m;
            foreach (var account in portfolio.Accounts)
                divNet += SumAccountNetDividendsForDay(account, date, reportingCurrency);

            return new ValuationRecord
            {
                Date = date,
                ReportingCurrency = reportingCurrency,
                Value = total,
                PortfolioId = portfolio.Id,
                SecuritiesValue = secs,
                CashValue = cash,
                IncomeForDay = divNet == 0m ? null : new Money(divNet, reportingCurrency)
            };
        }

        public async Task<ValuationRecord> GenerateAccountValuationSnapshot(
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

            var divNet = SumAccountNetDividendsForDay(account, date, reportingCurrency);

            return new ValuationRecord
            {
                Date = date,
                ReportingCurrency = reportingCurrency,
                Value = total,
                AccountId = account.Id,
                SecuritiesValue = secs,
                CashValue = cash,
                IncomeForDay = divNet == 0m ? null : new Money(divNet, reportingCurrency)
            };
        }

        public async Task<IEnumerable<ValuationRecord>> GeneratePortfolioAssetClassValuationSnapshot(
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
            if (total <= 0m) return Enumerable.Empty<ValuationRecord>();

            var byClass = await AggregateByAssetClassAsync(portfolio, date, reportingCurrency, ct);
            return byClass.Select(kvp => new ValuationRecord
            {
                Date = date,
                ReportingCurrency = reportingCurrency,
                Value = kvp.Value,
                PortfolioId = portfolio.Id,
                AssetClass = kvp.Key,
                Percentage = kvp.Value.Amount / total
            }).ToList();
        }

        public async Task<IEnumerable<ValuationRecord>> GenerateAccountAssetClassValuationSnapshot(
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
            if (total <= 0m) return Enumerable.Empty<ValuationRecord>();

            var byClass = await AggregateByAssetClassAsync(account, date, reportingCurrency, ct);
            return byClass.Select(kvp => new ValuationRecord
            {
                Date = date,
                ReportingCurrency = reportingCurrency,
                Value = kvp.Value,
                AccountId = account.Id,
                AssetClass = kvp.Key,
                Percentage = kvp.Value.Amount / total
            }).ToList();
        }

        // ---------------- STORAGE ----------------

        public async Task StorePortfolioValuation(int portfolioId, ValuationRecord valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            valuation.Period = period;
            await _valuationRepository.SaveAsync(valuation, ct);
        }

        public async Task StoreAccountValuation(int portfolioId, int accountId, ValuationRecord valuation, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            valuation.Period = period;
            await _valuationRepository.SaveAsync(valuation, ct);
        }

        public async Task StorePortfolioAssetClassValuation(int portfolioId, IEnumerable<ValuationRecord> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            foreach (var v in valuations)
            {
                v.Period = period;
                await _valuationRepository.SaveAsync(v, ct);
            }
        }

        public async Task StoreAccountAssetClassValuation(int portfolioId, int accountId, IEnumerable<ValuationRecord> valuations, DateOnly date, ValuationPeriod period, CancellationToken ct = default)
        {
            foreach (var v in valuations)
            {
                v.Period = period;
                await _valuationRepository.SaveAsync(v, ct);
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

        private decimal SumAccountNetDividendsForDay(Account account, DateOnly date, Currency reportingCurrency)
        {
            var net = account.Transactions
                .Where(t => t.Type == TransactionType.Dividend && t.Date == date)
                .Where(t => t.Amount.Currency == reportingCurrency)
                .Sum(t => (t.Amount.Amount - (t.Costs?.Amount ?? 0m)));
            return net;
        }

        //read
        public async Task<ValuationRecord?> GetLatestAsync(
            EntityKind kind,
            int entityId,
            Currency currency,
            ValuationPeriod? period = null,
            bool includeAssetClass = false,
            CancellationToken ct = default)
        {
            return await _valuationRepository.GetLatestAsync(kind, entityId, currency, period, includeAssetClass, ct);
        }

        public async Task<IEnumerable<ValuationRecord>> GetHistoryAsync(
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

        public async Task<IEnumerable<ValuationRecord>> GetAsOfDateAsync(
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
