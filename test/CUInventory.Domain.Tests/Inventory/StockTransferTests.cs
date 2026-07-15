using System;
using System.Linq;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Events;
using CUInventory.Inventory.Exceptions;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Inventory;

public class StockTransferTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;
    private static readonly Guid Source = Guid.NewGuid();
    private static readonly Guid Destination = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();

    private static StockTransfer NewTransfer(decimal quantity = 4m)
        => StockTransferTestFactory.NewTransfer(Source, Destination, Product, quantity);

    [Fact]
    public void Constructor_With_Same_Source_And_Destination_Warehouse_Throws()
    {
        Should.Throw<StockTransferSameWarehouseDomainException>(
            () => new StockTransfer(Guid.NewGuid(), Source, Source, []));
    }

    [Fact]
    public void Constructor_Creates_Draft_Transfer_With_Lines()
    {
        var lineId = Guid.NewGuid();
        var transfer = new StockTransfer(
            Guid.NewGuid(), Source, Destination, [new StockTransferLineData(lineId, Product, new Quantity(4m))]);

        transfer.ShouldSatisfyAllConditions(
            () => transfer.Status.ShouldBe(StockTransferStatus.Draft),
            () => transfer.SourceWarehouseId.ShouldBe(Source),
            () => transfer.DestinationWarehouseId.ShouldBe(Destination),
            () => transfer.Lines.ShouldHaveSingleItem(),
            () => transfer.Lines.Single().Id.ShouldBe(lineId),
            () => transfer.Allocations.ShouldBeEmpty());
    }

    [Fact]
    public void MarkDispatched_Sets_Dispatched_Status_And_Raises_TransferStarted()
    {
        var transfer = NewTransfer();

        transfer.MarkDispatched(Now);

        transfer.ShouldSatisfyAllConditions(
            () => transfer.Status.ShouldBe(StockTransferStatus.Dispatched),
            () => transfer.DispatchedAt.ShouldBe(Now),
            () => transfer.GetLocalEvents().Select(e => e.EventData).OfType<TransferStartedDomainEvent>().ShouldHaveSingleItem());
    }

    [Fact]
    public void MarkDispatched_When_Not_Draft_Throws()
    {
        var transfer = NewTransfer();
        transfer.MarkDispatched(Now);

        Should.Throw<StockTransferNotInDraftStateDomainException>(() => transfer.MarkDispatched(Now));
    }

    [Fact]
    public void MarkDispatched_With_No_Lines_Throws()
    {
        var transfer = new StockTransfer(Guid.NewGuid(), Source, Destination, []);

        Should.Throw<StockTransferHasNoLinesDomainException>(() => transfer.MarkDispatched(Now));
    }

    [Fact]
    public void MarkReceived_Sets_Received_Status_And_Raises_TransferCompleted()
    {
        var transfer = NewTransfer();
        transfer.MarkDispatched(Now);

        transfer.MarkReceived(Now.AddDays(1));

        transfer.ShouldSatisfyAllConditions(
            () => transfer.Status.ShouldBe(StockTransferStatus.Received),
            () => transfer.ReceivedAt.ShouldBe(Now.AddDays(1)),
            () => transfer.GetLocalEvents().Select(e => e.EventData).OfType<TransferCompletedDomainEvent>().ShouldHaveSingleItem());
    }

    [Fact]
    public void MarkReceived_When_Not_Dispatched_Throws()
    {
        var transfer = NewTransfer();

        Should.Throw<StockTransferNotDispatchedDomainException>(() => transfer.MarkReceived(Now));
    }

    [Fact]
    public void MarkCancelled_From_Draft_Sets_Cancelled()
    {
        var transfer = NewTransfer();

        transfer.MarkCancelled();

        transfer.Status.ShouldBe(StockTransferStatus.Cancelled);
    }

    [Fact]
    public void MarkCancelled_From_Dispatched_Sets_Cancelled()
    {
        var transfer = NewTransfer();
        transfer.MarkDispatched(Now);

        transfer.MarkCancelled();

        transfer.Status.ShouldBe(StockTransferStatus.Cancelled);
    }

    [Fact]
    public void MarkCancelled_When_Received_Throws()
    {
        var transfer = NewTransfer();
        transfer.MarkDispatched(Now);
        transfer.MarkReceived(Now);

        Should.Throw<StockTransferCannotBeCancelledDomainException>(() => transfer.MarkCancelled());
    }

    [Fact]
    public void MarkCancelled_When_Already_Cancelled_Throws()
    {
        var transfer = NewTransfer();
        transfer.MarkCancelled();

        Should.Throw<StockTransferCannotBeCancelledDomainException>(() => transfer.MarkCancelled());
    }
}
