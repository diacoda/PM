using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces
{
    public interface ICashFlowRepository
    {
        Task RecordCashFlowAsync(CashFlow flow, CancellationToken ct = default);
        Task<IEnumerable<CashFlow>> GetCashFlowsAsync(int accountId, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
        Task<Money> GetNetCashFlowAsync(int accountId, Currency currency, DateTime? from = null, DateTime? to = null, CancellationToken ct = default);
    }
}
