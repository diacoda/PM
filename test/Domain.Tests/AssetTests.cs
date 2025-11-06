using System;
using Xunit;
using PM.Domain.Entities;
using PM.Domain.Values;

namespace PM.Tests.Domain.Entities
{
    public class AssetTests
    {
        [Fact]
        public void Equals_ShouldReturnTrue_ForSameCodeAndCurrency()
        {
            var asset1 = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };
            var asset2 = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };

            Assert.True(asset1.Equals(asset2));
        }

        [Fact]
        public void Equals_ShouldIgnoreCase_ForCodeAndCurrency()
        {
            var asset1 = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };
            var asset2 = new Asset { Code = "vfv.to", Currency = new Currency("cad"), AssetClass = AssetClass.USEquity };

            Assert.True(asset1.Equals(asset2));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentCode()
        {
            var asset1 = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };
            var asset2 = new Asset { Code = "VCE.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };

            Assert.False(asset1.Equals(asset2));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_ForDifferentCurrency()
        {
            var asset1 = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };
            var asset2 = new Asset { Code = "VFV.TO", Currency = new Currency("USD"), AssetClass = AssetClass.USEquity };

            Assert.False(asset1.Equals(asset2));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenObjectIsNotAsset()
        {
            var asset = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };
            Assert.False(asset.Equals("NotAnAsset"));
        }

        [Fact]
        public void GetHashCode_ShouldBeSame_ForEqualAssets()
        {
            var asset1 = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };
            var asset2 = new Asset { Code = "vfv.to", Currency = new Currency("cad"), AssetClass = AssetClass.USEquity };

            Assert.Equal(asset1.GetHashCode(), asset2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_ShouldDiffer_ForDifferentAssets()
        {
            var asset1 = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };
            var asset2 = new Asset { Code = "VCE.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };

            Assert.NotEqual(asset1.GetHashCode(), asset2.GetHashCode());
        }

        [Fact]
        public void Equals_ShouldReturnTrue_WhenComparingAssetAndSymbolWithSameValues()
        {
            var asset = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };
            var symbol = new Symbol("VFV.TO", "CAD");

            Assert.True(asset.Equals(symbol));
            Assert.True(symbol.Equals(asset)); // ✅ symmetric check
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenComparingAssetAndSymbolWithDifferentCode()
        {
            var asset = new Asset { Code = "VFV.TO", Currency = new Currency("CAD"), AssetClass = AssetClass.USEquity };
            var symbol = new Symbol("VCE.TO", "CAD");

            Assert.False(asset.Equals(symbol));
            Assert.False(symbol.Equals(asset));
        }
    }
}
