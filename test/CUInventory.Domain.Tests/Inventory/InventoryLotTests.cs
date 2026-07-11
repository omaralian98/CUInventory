using System;
using CUInventory.Common.Exceptions;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Exceptions;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Inventory;

public class InventoryLotTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;

    private static InventoryLot NewLot(decimal quantity = 10m)
        => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), InventoryLotSource.Purchase, new Quantity(quantity), new Money(5m), Now, Guid.NewGuid());

    [Fact]
    public void Consume_Reduces_Remaining_Quantity()
    {
        var lot = NewLot(10m);

        lot.Consume(new Quantity(4m));

        lot.ShouldSatisfyAllConditions(
            () => lot.RemainingQuantity.Value.ShouldBe(6m),
            () => lot.OriginalQuantity.Value.ShouldBe(10m));
    }

    [Fact]
    public void Consume_Throws_When_More_Than_Remaining()
    {
        var lot = NewLot(10m);

        var ex = Should.Throw<InventoryLotInsufficientRemainingDomainException>(() => lot.Consume(new Quantity(11m)));
        ex.Code.ShouldBe(CUInventoryDomainErrorCodes.InventoryLotInsufficientRemaining);
    }

    [Fact]
    public void Restore_Returns_Consumed_Quantity()
    {
        var lot = NewLot(10m);
        lot.Consume(new Quantity(4m));

        lot.Restore(new Quantity(4m));

        lot.RemainingQuantity.Value.ShouldBe(10m);
    }

    [Fact]
    public void Restore_Throws_When_Exceeding_Original()
    {
        var lot = NewLot(10m);

        Should.Throw<InventoryLotRestoreExceedsOriginalDomainException>(() => lot.Restore(new Quantity(1m)));
    }

    [Fact]
    public void Rejects_Non_Positive_Original_Quantity()
    {
        Should.Throw<ArgumentMustBePositiveDomainException>(() => NewLot(0m));
    }
}
