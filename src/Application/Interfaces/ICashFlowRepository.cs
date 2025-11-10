using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces
{
    public interface ICashFlowRepository
    {
        Task<CashFlow> RecordCashFlowAsync(CashFlow flow, CancellationToken ct = default);
        Task<IEnumerable<CashFlow>> GetCashFlowsAsync(int accountId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);
        Task<Money> GetNetCashFlowAsync(int accountId, Currency currency, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);
    }
}
