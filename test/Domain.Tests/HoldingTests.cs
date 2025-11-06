using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using PM.Domain.Entities;
using PM.Domain.Interfaces;
using PM.Domain.Values;

namespace PM.Tests.Domain.Entities;

public class HoldingTests
{
    private Mock<IAsset> CreateAssetMock(string code = "VFV.TO", string currency = "CAD", string assetClass = "USEquity")
    {
        var mock = new Mock<IAsset>();
        mock.Setup(a => a.Code).Returns(code);
        mock.Setup(a => a.Currency).Returns(new Currency(currency));
        mock.Setup(a => a.AssetClass).Returns(Enum.Parse<AssetClass>(assetClass));
        return mock;
    }

    [Fact]
    public void Constructor_ShouldInitializeHolding()
    {
        var assetMock = CreateAssetMock();
        var holding = new Holding(assetMock.Object, 100);

        Assert.Equal("VFV.TO", holding.Asset.Code);
        Assert.Equal("CAD", holding.Asset.Currency.Code);
        Assert.Equal(AssetClass.USEquity, holding.Asset.AssetClass);
        Assert.Equal(100, holding.Quantity);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenAssetIsNull()
    {
        Assert.Throws<NullReferenceException>(() => new Holding(null!, 10));
    }

    [Fact]
    public void AssetProperty_ShouldUpdateAsset()
    {
        var assetMock = CreateAssetMock();
        var holding = new Holding(assetMock.Object, 50);

        var newAssetMock = CreateAssetMock("VCE.TO", "CAD", "CanadianEquity");
        holding.Asset = newAssetMock.Object;

        Assert.Equal("VCE.TO", holding.Asset.Code);
    }

    [Fact]
    public void AddQuantity_ShouldIncreaseQuantity()
    {
        var holding = new Holding(CreateAssetMock().Object, 10);
        holding.AddQuantity(5);
        Assert.Equal(15, holding.Quantity);
    }

    [Fact]
    public void UpdateQuantity_ShouldSetQuantity()
    {
        var holding = new Holding(CreateAssetMock().Object, 10);
        holding.UpdateQuantity(20);
        Assert.Equal(20, holding.Quantity);
    }

    [Fact]
    public void AddTag_ShouldAddNewTag()
    {
        var holding = new Holding(CreateAssetMock().Object, 10);
        var tag = new Tag("Growth");

        holding.AddTag(tag);

        Assert.Contains(tag, holding.Tags);
    }

    [Fact]
    public void AddTag_ShouldNotAddDuplicateTag()
    {
        var holding = new Holding(CreateAssetMock().Object, 10);
        var tag = new Tag("Growth");

        holding.AddTag(tag);
        holding.AddTag(tag);

        Assert.Single(holding.Tags);
    }

    [Fact]
    public void RemoveTag_ShouldRemoveExistingTag()
    {
        var holding = new Holding(CreateAssetMock().Object, 10);
        var tag = new Tag("Growth");

        holding.AddTag(tag);
        holding.RemoveTag(tag);

        Assert.DoesNotContain(tag, holding.Tags);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameAsset()
    {
        var assetMock = CreateAssetMock();
        var holding1 = new Holding(assetMock.Object, 10);
        var holding2 = new Holding(assetMock.Object, 20);

        Assert.True(holding1.Equals(holding2));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentAsset()
    {
        var holding1 = new Holding(CreateAssetMock("VFV.TO").Object, 10);
        var holding2 = new Holding(CreateAssetMock("VCE.TO").Object, 10);

        Assert.False(holding1.Equals(holding2));
    }

    [Fact]
    public void GetHashCode_ShouldMatchAssetHashCode()
    {
        var assetMock = CreateAssetMock();
        var holding = new Holding(assetMock.Object, 10);

        Assert.Equal(holding.Asset.GetHashCode(), holding.GetHashCode());
    }
}
