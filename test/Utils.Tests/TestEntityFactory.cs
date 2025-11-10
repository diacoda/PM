using System.Collections.Generic;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Utils.Tests
{
    /// <summary>
    /// Provides quick factory methods to create realistic domain entities for tests.
    /// </summary>
    public static class TestEntityFactory
    {
        private static int _nextId = 1;

        /// <summary>
        /// Resets ID counter between test runs.
        /// </summary>
        public static void ResetIds() => _nextId = 1;

        /// <summary>
        /// Creates an Account with a given name and currency.
        /// </summary>
        public static Account CreateAccount(string name, Currency currency)
        {
            var acc = new Account(name, currency, FinancialInstitutions.TD);
            acc.SetIdForTest(_nextId++);
            return acc;
        }

        /// <summary>
        /// Creates an Account with explicit financial institution.
        /// </summary>
        public static Account CreateAccount(string name, Currency currency, FinancialInstitutions institution)
        {
            var acc = new Account(name, currency, institution);
            acc.SetIdForTest(_nextId++);
            return acc;
        }

        /// <summary>
        /// Creates a Tag entity for tagging holdings or accounts.
        /// </summary>
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

        /// <summary>
        /// Creates a Holding for the given symbol and quantity.
        /// </summary>
        public static Holding CreateHolding(Symbol symbol, decimal quantity)
        {
            var holding = new Holding(symbol, quantity);
            holding.SetIdForTest(_nextId++);
            return holding;
        }

        public static CashFlow CreateCashFlow(int accountId, Money amount, CashFlowType type, string note)
        {
            var flow = new CashFlow
            {
                AccountId = accountId,
                Amount = amount,
                Type = type,
                Note = note
            };
            flow.SetIdForTest(_nextId++);
            return flow;
        }

        /// <summary>
        /// Creates an Account pre-populated with a Holding.
        /// </summary>
        public static Account CreateAccountWithHolding(string name, Currency currency, Symbol symbol, decimal qty)
        {
            var account = CreateAccount(name, currency);
            var holding = CreateHolding(symbol, qty);
            account.UpsertHolding(holding);
            return account;
        }

        /// <summary>
        /// Creates a Portfolio with a given name and optional list of Accounts.
        /// </summary>
        public static Portfolio CreatePortfolio(string name, params Account[] accounts)
        {
            var portfolio = new Portfolio(name);
            portfolio.SetIdForTest(_nextId++);
            foreach (var acc in accounts)
                portfolio.AddAccount(acc);
            return portfolio;
        }

        /// <summary>
        /// Creates a Portfolio pre-populated with a few Accounts and Holdings — useful for integration tests.
        /// </summary>
        public static Portfolio CreatePortfolioWithAccountsAndHoldings()
        {
            var usd = new Currency("USD");
            var cad = new Currency("CAD");

            var acc1 = CreateAccountWithHolding("RRSP", usd, new Symbol("VFV.TO", "CAD"), 10m);
            var acc2 = CreateAccountWithHolding("TFSA", cad, new Symbol("VCE.TO", "CAD"), 5m);

            return CreatePortfolio("My Portfolio", acc1, acc2);
        }

        public static Transaction CreateTransaction(
            int accountId,
            TransactionType type,
            Symbol instrument,
            decimal quantity,
            Money amount,
            DateOnly date)
        {
            var tx = new Transaction
            {
                AccountId = accountId,
                Type = type,
                Symbol = instrument,
                Quantity = quantity,
                Amount = amount,
                Date = date
            };
            tx.SetIdForTest(_nextId++);
            return tx;
        }

        public static Transaction CreateTransaction(
            int accountId,
            TransactionType type,
            Symbol instrument,
            decimal quantity,
            Money amount,
            Money costs)
        {
            var tx = new Transaction
            {
                AccountId = accountId,
                Type = type,
                Symbol = instrument,
                Quantity = quantity,
                Amount = amount,
                Costs = costs,
                Date = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            tx.SetIdForTest(_nextId++);
            return tx;
        }
    }
}
