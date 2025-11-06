using Xunit;
using FluentAssertions;
using PM.Domain.Entities;
using PM.Domain.Values;
using System.Linq;

namespace Domain.Tests
{
    public class AccountTests
    {
        [Fact]
        public void UpsertHolding_ShouldMergeQuantities_WhenSymbolExists()
        {
            var account = new Account("Cash", new Currency("CAD"), PM.Domain.Enums.FinancialInstitutions.TD);
            var symbol = new Symbol("VFV.TO");
            account.UpsertHolding(new Holding(symbol, 10));
            account.UpsertHolding(new Holding(symbol, 5));

            account.Holdings.Should().ContainSingle(h => h.Asset.Code == "VFV.TO");
            account.Holdings.FirstOrDefault().Quantity.Should().Be(15);
        }
    }
}
