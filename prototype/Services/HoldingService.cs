using model.Domain.Entities;
using model.Domain.Values;

namespace model.Services;

public class HoldingService
{
    public void AddHolding(Account account, Instrument instrument, decimal quantity)
    {
        var existing = account.Holdings.FirstOrDefault(h => h.Instrument.Symbol == instrument.Symbol);
        if (existing != null)
        {
            existing.Quantity += quantity;
        }
        else
        {
            //account.Holdings.Add(new Holding { Instrument = instrument, Quantity = quantity });
            account.AddHolding(new Holding { Instrument = instrument, Quantity = quantity });
        }
    }

    public void RemoveHolding(Account account, Symbol symbol)
    {
        var holding = account.Holdings.FirstOrDefault(h => h.Instrument.Symbol == symbol);
        if (holding != null)
        {
            //account.Holdings.Remove(holding);
            account.RemoveHolding(holding);
        }
    }

    public Holding? GetHolding(Account account, Symbol symbol)
    {
        return account.Holdings.FirstOrDefault(h => h.Instrument.Symbol == symbol);
    }

    public void UpdateHoldingQuantity(Account account, Symbol symbol, decimal newQuantity)
    {
        var holding = GetHolding(account, symbol);
        if (holding != null)
        {
            holding.Quantity = newQuantity;
        }
    }

    public decimal GetCashBalance(Account account, Currency currency)
    {
        var symbol = Symbol.From($"CASH.{currency}");
        var holding = account.Holdings.FirstOrDefault(h => h.Instrument.Symbol == symbol);
        return holding?.Quantity ?? 0;
    }

    public IEnumerable<Holding> ListHoldings(Account account)
    {
        return account.Holdings;
    }

    public void AddTag(Holding holding, Tag tag)
    {
        if (!holding.Tags.Contains(tag))
            holding.Tags.Add(tag);
    }

    public void RemoveTag(Holding holding, Tag tag)
    {
        holding.Tags.Remove(tag);
    }
}