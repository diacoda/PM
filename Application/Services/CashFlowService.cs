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

        public async Task RecordCashFlowAsync(Account account, DateTime date, Money amount, CashFlowType type, string? note = null)
        {
            var flow = new CashFlow
            {
                AccountId = account.Id,
                Date = date,
                Amount = amount,
                Type = type,
                Note = note
            };

            await _repo.RecordCashFlowAsync(flow);
        }

        public async Task<IEnumerable<CashFlow>> GetCashFlowsAsync(Account account, DateTime? from = null, DateTime? to = null)
            => await _repo.GetCashFlowsAsync(account.Id, from, to);

        public async Task<Money> GetNetCashFlowAsync(Account account, Currency currency, DateTime? from = null, DateTime? to = null)
            => await _repo.GetNetCashFlowAsync(account.Id, currency, from, to);

        public async Task<Money> GetPortfolioNetCashFlowAsync(Portfolio portfolio, Currency currency, DateTime? from = null, DateTime? to = null)
        {
            decimal total = 0;

            foreach (var account in portfolio.Accounts)
            {
                var net = await _repo.GetNetCashFlowAsync(account.Id, currency, from, to);
                total += net.Amount;
            }

            return new Money(total, currency);
        }
    }
}
