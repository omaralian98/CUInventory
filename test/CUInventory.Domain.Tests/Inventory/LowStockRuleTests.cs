using System;
using CUInventory.Inventory.Aggregates;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Inventory;

public class LowStockRuleTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;

    [Theory]
    [InlineData(null, 5.0, false)]
    [InlineData(10.0, 5.0, true)]
    [InlineData(10.0, 10.0, false)]
    [InlineData(10.0, 15.0, false)]
    public void IsBelowThreshold_Compares_Available_Against_The_Threshold(double? threshold, double available, bool expected)
    {
        LowStockRule.IsBelowThreshold((decimal?)threshold, (decimal)available).ShouldBe(expected);
    }

    [Fact]
    public void BelowThreshold_Expression_Matches_Balances_Under_Their_Threshold()
    {
        var below = new InventoryBalance(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 8m);
        below.Increase(new Quantity(10m), Now);
        below.Reserve(new Quantity(3m), Now);

        var above = new InventoryBalance(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 8m);
        above.Increase(new Quantity(10m), Now);

        var noThreshold = new InventoryBalance(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var isBelow = LowStockRule.BelowThreshold.Compile();
        isBelow(below).ShouldBeTrue();
        isBelow(above).ShouldBeFalse();
        isBelow(noThreshold).ShouldBeFalse();
    }
}
