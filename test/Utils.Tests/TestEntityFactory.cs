using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Utils.Tests;

public static class TestEntityFactory
{
    private static int _nextId = 1;

    public static Account CreateAccount(string name, Currency currency)
    {
        var acc = new Account(name, currency, FinancialInstitutions.TD);
        acc.SetIdForTest(_nextId++);
        return acc;
    }
    public static Account CreateAccount(string name, Currency currency, FinancialInstitutions financialInstitution)
    {
        var acc = new Account(name, currency, financialInstitution);
        acc.SetIdForTest(_nextId++);
        return acc;
    }

    public static Tag CreateTag(string name)
    {
        var tag = new Tag(name);
        tag.SetIdForTest(_nextId++);
        return tag;
    }

    public static Tag CreateTag(string name, int id)
    {
        var tag = new Tag(name);
        tag.SetIdForTest(id);
        return tag;
    }

    public static Holding CreateHolding(Symbol symbol, decimal quantity)
    {
        var holding = new Holding(symbol, quantity);
        holding.SetIdForTest(_nextId++);
        return holding;
    }
}
