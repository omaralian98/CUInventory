using System;
using System.Linq;
using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Events;
using CUInventory.Warehousing.Exceptions;
using Shouldly;
using Xunit;

namespace CUInventory.Warehousing;

public class ShipmentTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;

    private static Shipment NewShipment(bool withLine = true)
    {
        ShipmentLineData[] lines = withLine
            ? [new ShipmentLineData(Guid.NewGuid(), Guid.NewGuid(), new Quantity(10m), new Money(5m))]
            : [];

        return new Shipment(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), lines);
    }

    [Fact]
    public void Dispatch_Moves_To_Dispatched_And_Raises_The_Event()
    {
        var shipment = NewShipment();

        shipment.Dispatch(Now);

        shipment.ShouldSatisfyAllConditions(
            () => shipment.Status.ShouldBe(ShipmentStatus.Dispatched),
            () => shipment.DispatchedAt.ShouldBe(Now),
            () => shipment.GetLocalEvents().Select(e => e.EventData).OfType<ShipmentDispatchedDomainEvent>().ShouldHaveSingleItem());
    }

    [Fact]
    public void Dispatch_Throws_When_There_Are_No_Lines()
    {
        var shipment = NewShipment(withLine: false);

        Should.Throw<ShipmentHasNoLinesDomainException>(() => shipment.Dispatch(Now));
    }

    [Fact]
    public void MarkReceived_Throws_When_Not_Dispatched()
    {
        var shipment = NewShipment();

        Should.Throw<ShipmentNotDispatchedDomainException>(() => shipment.MarkReceived(Now));
    }
}
