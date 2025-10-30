using System.Threading.Tasks;
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Application.Services;

public class CashService : ICashService
{
    private readonly IHoldingService _holdingService;

    public CashService(IHoldingService holdingService)
    {
        _holdingService = holdingService;
    }

    public async Task DepositAsync(Account account, Money amount)
    {
        var symbol = new Symbol(amount.Currency.Code);
        await _holdingService.UpsertHoldingAsync(account.Id, symbol, amount.Amount);
    }

    public async Task WithdrawAsync(Account account, Money amount)
    {

        var symbol = new Symbol(amount.Currency.Code);
        var holding = account.Holdings.FirstOrDefault(h => h.Symbol == symbol);
        if (holding == null)
            throw new InvalidOperationException($"No cash holding found for {symbol}");

        if (holding.Quantity < amount.Amount)
            throw new InvalidOperationException($"Insufficient funds in {symbol}");

        decimal newQty = holding.Quantity - amount.Amount;
        holding.Quantity = newQty;
        await _holdingService.UpdateHoldingQuantityAsync(holding, newQty);
    }

    public async Task<decimal> GetBalanceAsync(int accountId, Currency currency)
    {
        return await _holdingService.GetCashBalanceAsync(accountId, currency);
    }
}