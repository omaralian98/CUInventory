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

    [Fact]
    public void Reserve_Reduces_Available_Without_Touching_Remaining()
    {
        var lot = NewLot(10m);

        lot.Reserve(new Quantity(4m));

        lot.ShouldSatisfyAllConditions(
            () => lot.ReservedQuantity.Value.ShouldBe(4m),
            () => lot.RemainingQuantity.Value.ShouldBe(10m),
            () => lot.AvailableQuantity.ShouldBe(6m));
    }

    [Fact]
    public void Reserve_Throws_When_Exceeding_Available()
    {
        var lot = NewLot(10m);
        lot.Reserve(new Quantity(7m));

        var ex = Should.Throw<InventoryLotInsufficientAvailableDomainException>(() => lot.Reserve(new Quantity(4m)));
        ex.Code.ShouldBe(CUInventoryDomainErrorCodes.InventoryLotInsufficientAvailable);
    }

    [Fact]
    public void Consume_Cannot_Take_Reserved_Stock()
    {
        var lot = NewLot(10m);
        lot.Reserve(new Quantity(7m));

        Should.Throw<InventoryLotInsufficientRemainingDomainException>(() => lot.Consume(new Quantity(4m)));
    }

    [Fact]
    public void ConsumeReserved_Reduces_Remaining_And_Reserved()
    {
        var lot = NewLot(10m);
        lot.Reserve(new Quantity(7m));

        lot.ConsumeReserved(new Quantity(7m));

        lot.ShouldSatisfyAllConditions(
            () => lot.ReservedQuantity.Value.ShouldBe(0m),
            () => lot.RemainingQuantity.Value.ShouldBe(3m),
            () => lot.AvailableQuantity.ShouldBe(3m));
    }

    [Fact]
    public void ConsumeReserved_Throws_When_Exceeding_Reserved()
    {
        var lot = NewLot(10m);
        lot.Reserve(new Quantity(2m));

        var ex = Should.Throw<InventoryLotInsufficientReservedDomainException>(() => lot.ConsumeReserved(new Quantity(3m)));
        ex.Code.ShouldBe(CUInventoryDomainErrorCodes.InventoryLotInsufficientReserved);
    }

    [Fact]
    public void ReleaseReservation_Restores_Available()
    {
        var lot = NewLot(10m);
        lot.Reserve(new Quantity(7m));

        lot.ReleaseReservation(new Quantity(7m));

        lot.ShouldSatisfyAllConditions(
            () => lot.ReservedQuantity.Value.ShouldBe(0m),
            () => lot.AvailableQuantity.ShouldBe(10m));
    }

    [Fact]
    public void ReleaseReservation_Throws_When_Exceeding_Reserved()
    {
        var lot = NewLot(10m);

        Should.Throw<InventoryLotInsufficientReservedDomainException>(() => lot.ReleaseReservation(new Quantity(1m)));
    }

    [Fact]
    public void Restore_Leaves_Reserved_Untouched()
    {
        var lot = NewLot(10m);
        lot.Reserve(new Quantity(3m));
        lot.Consume(new Quantity(4m));

        lot.Restore(new Quantity(4m));

        lot.ShouldSatisfyAllConditions(
            () => lot.RemainingQuantity.Value.ShouldBe(10m),
            () => lot.ReservedQuantity.Value.ShouldBe(3m),
            () => lot.AvailableQuantity.ShouldBe(7m));
    }
}
