using Microsoft.EntityFrameworkCore;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.Application.Interfaces;
using PM.Infrastructure.Data;
using PM.Domain.Enums;

namespace PM.Infrastructure.Repositories
{
    public class CashFlowRepository : ICashFlowRepository
    {
        private readonly CashFlowDbContext _context;

        public CashFlowRepository(CashFlowDbContext context)
        {
            _context = context;
        }

        public async Task RecordCashFlowAsync(CashFlow flow, CancellationToken ct = default)
        {
            _context.CashFlows.Add(flow);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<IEnumerable<CashFlow>> GetCashFlowsAsync(int accountId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
        {
            var query = _context.CashFlows
                .AsNoTracking()
                .Where(f => f.AccountId == accountId);

            if (from.HasValue)
                query = query.Where(f => f.Date >= from.Value);
            if (to.HasValue)
                query = query.Where(f => f.Date <= to.Value);

            return await query.OrderBy(f => f.Date).ToListAsync(ct);
        }

        public async Task<Money> GetNetCashFlowAsync(int accountId, Currency currency, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
        {
            var flows = await GetCashFlowsAsync(accountId, from, to, ct);
            var sameCurrency = flows.Where(f => f.Amount.Currency == currency);

            var total = sameCurrency.Sum(f =>
                f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee
                    ? -f.Amount.Amount
                    : f.Amount.Amount);

            return new Money(total, currency);
        }
    }
}
