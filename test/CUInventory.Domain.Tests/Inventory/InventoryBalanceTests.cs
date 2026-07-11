using System;
using System.Linq;
using CUInventory.Common.Exceptions;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Events;
using CUInventory.Inventory.Exceptions;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Inventory;

public class InventoryBalanceTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;

    private static InventoryBalance NewBalance(decimal onHand = 0m, decimal? threshold = null)
    {
        var balance = new InventoryBalance(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), threshold);
        if (onHand > 0)
        {
            balance.Increase(new Quantity(onHand), Now);
        }

        return balance;
    }

    [Fact]
    public void Increase_Adds_To_On_Hand_And_Raises_StockChanged()
    {
        var balance = NewBalance();

        balance.Increase(new Quantity(10m), Now);

        balance.ShouldSatisfyAllConditions(
            () => balance.QuantityOnHand.ShouldBe(10m),
            () => balance.QuantityAvailable.ShouldBe(10m),
            () => balance.GetLocalEvents().Select(e => e.EventData).OfType<StockChangedDomainEvent>().ShouldNotBeEmpty());
    }

    [Fact]
    public void Reserve_Reduces_Availability_Without_Touching_On_Hand()
    {
        var balance = NewBalance(onHand: 10m);

        balance.Reserve(new Quantity(4m), Now);

        balance.ShouldSatisfyAllConditions(
            () => balance.QuantityOnHand.ShouldBe(10m),
            () => balance.QuantityReserved.ShouldBe(4m),
            () => balance.QuantityAvailable.ShouldBe(6m));
    }

    [Fact]
    public void Reserve_Throws_When_Availability_Is_Insufficient()
    {
        var balance = NewBalance(onHand: 3m);

        var ex = Should.Throw<InsufficientStockDomainException>(() => balance.Reserve(new Quantity(4m), Now));
        ex.ShouldSatisfyAllConditions(
            () => ex.Code.ShouldBe(CUInventoryDomainErrorCodes.InsufficientStock),
            () => ex.Data["Requested"].ShouldBe(4m),
            () => ex.Data["Available"].ShouldBe(3m));
    }

    [Fact]
    public void Second_Reservation_Cannot_Oversell_Limited_Stock()
    {
        var balance = NewBalance(onHand: 1m);

        balance.Reserve(new Quantity(1m), Now);

        Should.Throw<InsufficientStockDomainException>(() => balance.Reserve(new Quantity(1m), Now));
        balance.QuantityReserved.ShouldBe(1m);
    }

    [Fact]
    public void ReleaseReservation_Returns_Stock_To_Availability()
    {
        var balance = NewBalance(onHand: 10m);
        balance.Reserve(new Quantity(4m), Now);

        balance.ReleaseReservation(new Quantity(4m), Now);

        balance.QuantityAvailable.ShouldBe(10m);
    }

    [Fact]
    public void ReleaseReservation_Throws_When_More_Than_Reserved()
    {
        var balance = NewBalance(onHand: 10m);
        balance.Reserve(new Quantity(4m), Now);

        var ex = Should.Throw<InsufficientReservedStockDomainException>(() => balance.ReleaseReservation(new Quantity(5m), Now));
        ex.Code.ShouldBe(CUInventoryDomainErrorCodes.InsufficientReservedStock);
    }

    [Fact]
    public void ConfirmReservation_Reduces_On_Hand_And_Reserved()
    {
        var balance = NewBalance(onHand: 10m);
        balance.Reserve(new Quantity(4m), Now);

        balance.ConfirmReservation(new Quantity(4m), Now);

        balance.ShouldSatisfyAllConditions(
            () => balance.QuantityOnHand.ShouldBe(6m),
            () => balance.QuantityReserved.ShouldBe(0m),
            () => balance.QuantityAvailable.ShouldBe(6m));
    }

    [Fact]
    public void DeductDirect_Throws_When_Availability_Is_Insufficient()
    {
        var balance = NewBalance(onHand: 3m);

        Should.Throw<InsufficientStockDomainException>(() => balance.DeductDirect(new Quantity(4m), Now));
    }

    [Fact]
    public void Crossing_The_Threshold_Raises_LowStockReached()
    {
        var balance = NewBalance(onHand: 10m, threshold: 5m);

        balance.Reserve(new Quantity(6m), Now);

        balance.GetLocalEvents().Select(e => e.EventData).OfType<LowStockReachedDomainEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Staying_Above_The_Threshold_Does_Not_Raise_LowStockReached()
    {
        var balance = NewBalance(onHand: 10m, threshold: 5m);

        balance.Reserve(new Quantity(2m), Now);

        balance.GetLocalEvents().Select(e => e.EventData).OfType<LowStockReachedDomainEvent>().ShouldBeEmpty();
    }

    [Fact]
    public void Reserve_Rejects_Non_Positive_Quantities()
    {
        var balance = NewBalance(onHand: 10m);

        Should.Throw<ArgumentMustBePositiveDomainException>(() => balance.Reserve(new Quantity(0m), Now));
    }
}
