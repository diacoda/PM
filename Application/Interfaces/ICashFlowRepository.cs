using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces
{
    public interface ICashFlowRepository
    {
        Task RecordCashFlowAsync(CashFlow flow);
        Task<IEnumerable<CashFlow>> GetCashFlowsAsync(int accountId, DateTime? from = null, DateTime? to = null);
        Task<Money> GetNetCashFlowAsync(int accountId, Currency currency, DateTime? from = null, DateTime? to = null);
    }
}
