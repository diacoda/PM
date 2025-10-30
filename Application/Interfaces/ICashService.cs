using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Interfaces;

public interface ICashService
{
    Task DepositAsync(Account account, Money amount);
    Task WithdrawAsync(Account account, Money amount);
    Task<decimal> GetBalanceAsync(int accountId, Currency currency);
}
