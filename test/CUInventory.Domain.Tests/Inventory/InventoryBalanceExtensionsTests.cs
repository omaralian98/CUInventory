using System;
using System.Collections.Generic;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Exceptions;
using Shouldly;
using Xunit;

namespace CUInventory.Inventory;

public class InventoryBalanceExtensionsTests
{
    [Fact]
    public void FindRequired_Returns_The_Matching_Balance()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var match = new InventoryBalance(Guid.NewGuid(), warehouseId, productId);
        var other = new InventoryBalance(Guid.NewGuid(), Guid.NewGuid(), productId);
        var balances = new List<InventoryBalance> { other, match };

        balances.FindRequired(warehouseId, productId).ShouldBeSameAs(match);
    }

    [Fact]
    public void FindRequired_Throws_When_No_Balance_Matches()
    {
        var balances = new List<InventoryBalance>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid())
        };

        Should.Throw<MissingInventoryBalanceDomainException>(
            () => balances.FindRequired(Guid.NewGuid(), Guid.NewGuid()));
    }
}
