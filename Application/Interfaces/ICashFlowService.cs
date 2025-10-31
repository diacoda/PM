using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface ICashFlowService
{
    Task RecordCashFlowAsync(int accountId, DateTime date, Money amount, CashFlowType type, string? note = null);
    Task<IEnumerable<CashFlow>> GetCashFlowsAsync(Account account, DateTime? from = null, DateTime? to = null);
    Task<Money> GetNetCashFlowAsync(Account account, Currency currency, DateTime? from = null, DateTime? to = null);
    Task<Money> GetPortfolioNetCashFlowAsync(Portfolio portfolio, Currency currency, DateTime? from = null, DateTime? to = null);
}
