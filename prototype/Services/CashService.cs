using model.Domain.Entities;
using model.Domain.Values;

namespace model.Services;
public class CashService
{
    private readonly HoldingService _holdingService;

    public CashService(HoldingService holdingService)
    {
        _holdingService = holdingService;
    }

    public void Deposit(Account account, Money amount)
    {
        var symbol = Symbol.From($"CASH.{amount.Currency}");
        var instrument = new Instrument(symbol, $"{amount.Currency} Cash", AssetClass.Cash);
        _holdingService.AddHolding(account, instrument, amount.Amount);
    }

    public void Withdraw(Account account, Money amount)
    {
        var symbol = Symbol.From($"CASH.{amount.Currency}");
        var holding = account.Holdings.FirstOrDefault(h => h.Instrument.Symbol == symbol);
        if (holding != null)
        {
            holding.Quantity -= amount.Amount;
        }
    }

    public decimal GetBalance(Account account, Currency currency)
    {
        return _holdingService.GetCashBalance(account, currency);
    }
}