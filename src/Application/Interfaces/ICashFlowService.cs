using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface ICashFlowService
{
    Task RecordCashFlowAsync(int accountId, DateOnly date, Money amount, CashFlowType type, string? note = null, CancellationToken ct = default);
    Task<IEnumerable<CashFlow>> GetCashFlowsAsync(Account account, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);
    Task<Money> GetNetCashFlowAsync(Account account, Currency currency, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);
    Task<Money> GetPortfolioNetCashFlowAsync(Portfolio portfolio, Currency currency, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default);
}
