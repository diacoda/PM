using model.Domain.Entities;
using model.Domain.Values;

namespace model.Services;

public class CashFlowService
{
    private readonly List<CashFlow> _flows = new();

    public void RecordCashFlow(Account account, DateTime date, Money amount, CashFlowType type, string? note = null)
    {
        var flow = new CashFlow
        {
            AccountId = account.Id,
            Date = date,
            Amount = amount,
            Type = type,
            Note = note
        };

        _flows.Add(flow);
    }

    public IEnumerable<CashFlow> GetCashFlows(Account account, DateTime? from = null, DateTime? to = null)
    {
        return _flows
            .Where(f => f.AccountId == account.Id &&
                        (!from.HasValue || f.Date >= from.Value) &&
                        (!to.HasValue || f.Date <= to.Value))
            .OrderBy(f => f.Date);
    }

    public Money GetNetCashFlow(Account account, Currency currency, DateTime? from = null, DateTime? to = null)
    {
        var flows = GetCashFlows(account, from, to)
            .Where(f => f.Amount.Currency == currency);

        var total = flows.Sum(f => f.Type == CashFlowType.Withdrawal || f.Type == CashFlowType.Fee
            ? -f.Amount.Amount
            : f.Amount.Amount);

        return new Money(total, currency);
    }

    public Money GetPortfolioNetCashFlow(Portfolio portfolio, Currency currency, DateTime? from = null, DateTime? to = null)
    {
        decimal total = 0;

        foreach (var account in portfolio.Accounts)
        {
            var net = GetNetCashFlow(account, currency, from, to);
            total += net.Amount;
        }

        return new Money(total, currency);
    }
}