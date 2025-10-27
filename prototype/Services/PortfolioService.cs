using model.Domain.Entities;
using model.Domain.Values;

namespace model.Services;

public class PortfolioService
{
    private readonly List<Portfolio> _portfolios = new();

    public Portfolio Create(string owner)
    {
        var portfolio = new Portfolio { Owner = owner };
        _portfolios.Add(portfolio);
        return portfolio;
    }

    public Portfolio? GetById(int id) => _portfolios.FirstOrDefault(p => p.Id == id);

    public IEnumerable<Portfolio> List() => _portfolios;

    public void UpdateOwner(int id, string newOwner)
    {
        var portfolio = GetById(id);
        if (portfolio != null)
        {
            portfolio.Owner = newOwner;
        }
    }

    public void Delete(int id)
    {
        var portfolio = GetById(id);
        if (portfolio != null)
        {
            _portfolios.Remove(portfolio);
        }
    }
    public IEnumerable<Account> GetAccountsByTag(Portfolio portfolio, Tag tag)
    {
        return portfolio.Accounts.Where(a => a.Tags.Contains(tag));
    }
}