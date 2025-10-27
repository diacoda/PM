using model.Domain.Entities;
using model.Domain.Values;

namespace model.Interfaces;

public interface IHoldingService
{
    public void AddHolding(Account account, Instrument instrument, decimal quantity);
    public void RemoveHolding(Account account, Symbol symbol);
    public Holding? GetHolding(Account account, Symbol symbol);
    public void UpdateHoldingQuantity(Account account, Symbol symbol, decimal newQuantity);
    public decimal GetCashBalance(Account account, Currency currency);
    public IEnumerable<Holding> ListHoldings(Account account);
    public void AddTag(Holding holding, Tag tag);
    public void RemoveTag(Holding holding, Tag tag);
}