using PM.Domain.Entities;
using PM.Domain.Values;

using PM.Application.Interfaces;
using PM.Domain.Enums;

namespace PM.Application.Services
{
    public class CashFlowService : ICashFlowService
    {
        private readonly ICashFlowRepository _repo;

        public CashFlowService(ICashFlowRepository repo)
        {
            _repo = repo;
        }

        public async Task<CashFlow> RecordCashFlowAsync(int accountId, DateOnly date, Money amount, CashFlowType type, string? note = null, CancellationToken ct = default)
        {
            var flow = new CashFlow
            {
                AccountId = accountId,
                Date = date,
                Amount = amount,
                Type = type,
                Note = note
            };

            return await _repo.RecordCashFlowAsync(flow, ct);
        }

        public async Task<CashFlow?> GetCashFlowByIdAsync(int cashFlowId, CancellationToken ct = default)
            => await _repo.GetCashFlowByIdAsync(cashFlowId, ct);

        public async Task DeleteCashFlowAsync(int cashFlowId, CancellationToken ct = default)
        {
            var flow = await _repo.GetCashFlowByIdAsync(cashFlowId, ct);
            if (flow is null)
                throw new Exception("Cash flow not found.");

            await _repo.DeleteCashFlowAsync(flow, ct);
        }

        public async Task<IEnumerable<CashFlow>> GetCashFlowsAsync(Account account, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
            => await _repo.GetCashFlowsAsync(account.Id, from, to, ct);

        public async Task<Money> GetNetCashFlowAsync(Account account, Currency currency, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
            => await _repo.GetNetCashFlowAsync(account.Id, currency, from, to, ct);

        public async Task<Money> GetPortfolioNetCashFlowAsync(Portfolio portfolio, Currency currency, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
        {
            decimal total = 0;

            foreach (var account in portfolio.Accounts)
            {
                var net = await _repo.GetNetCashFlowAsync(account.Id, currency, from, to, ct);
                total += net.Amount;
            }

            return new Money(total, currency);
        }
    }
}
