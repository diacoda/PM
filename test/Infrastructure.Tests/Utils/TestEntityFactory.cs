using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Infrastructure.Tests.Utils;

public static class TestEntityFactory
{
    private static int _nextId = 0;

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
}
