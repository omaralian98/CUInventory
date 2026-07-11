using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Allocation;
using CUInventory.Inventory.Exceptions;
using CUInventory.Inventory.Managers;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Inventory;

public class StockTransferManagerTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;
    private static readonly Guid Source = Guid.NewGuid();
    private static readonly Guid Destination = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();
    private static readonly Guid Supplier = Guid.NewGuid();

    private static StockTransferManager CreateManager()
    {
        var allocation = new InventoryAllocationService(
        [
            new FifoAllocationStrategy()
        ]);
        return new StockTransferManager(allocation).WithTestGuidGenerator();
    }

    private static StockTransfer NewTransfer(decimal quantity = 4m)
        => new(
            Guid.NewGuid(),
            Source,
            Destination,
            [new StockTransferLineData(Guid.NewGuid(), Product, new Quantity(quantity))]);

    private static InventoryBalance Balance(Guid warehouseId, decimal onHand)
    {
        var balance = new InventoryBalance(Guid.NewGuid(), warehouseId, Product);
        if (onHand > 0)
        {
            balance.Increase(new Quantity(onHand), Now);
        }

        return balance;
    }

    private static InventoryLot SourceLot(decimal quantity = 10m, decimal unitCost = 5m)
        => new(Guid.NewGuid(), Product, Source, InventoryLotSource.Purchase, new Quantity(quantity), new Money(unitCost), Now, Supplier);

    [Fact]
    public async Task DispatchAsync_Consumes_Source_Lots_And_Deducts_The_Source_Balance()
    {
        var manager = CreateManager();
        var transfer = NewTransfer();
        var sourceBalance = Balance(Source, 10m);
        var lot = SourceLot(10m);

        await manager.DispatchAsync(transfer, [sourceBalance], [lot]);

        transfer.ShouldSatisfyAllConditions(
            () => transfer.Status.ShouldBe(StockTransferStatus.Dispatched),
            () => transfer.Allocations.ShouldHaveSingleItem(),
            () => transfer.Allocations.Single().Quantity.Value.ShouldBe(4m),
            () => transfer.Allocations.Single().SupplierId.ShouldBe(Supplier),
            () => lot.RemainingQuantity.Value.ShouldBe(6m),
            () => sourceBalance.QuantityOnHand.ShouldBe(6m));
    }

    [Fact]
    public async Task ReceiveAsync_Recreates_Destination_Lots_Preserving_Traceability()
    {
        var manager = CreateManager();
        var transfer = NewTransfer();
        var sourceBalance = Balance(Source, 10m);
        await manager.DispatchAsync(transfer, [sourceBalance], [SourceLot(10m)]);
        var destinationBalance = Balance(Destination, 0m);

        var createdLots = await manager.ReceiveAsync(transfer, [destinationBalance]);

        createdLots.ShouldHaveSingleItem();
        var lot = createdLots.Single();
        lot.ShouldSatisfyAllConditions(
            () => transfer.Status.ShouldBe(StockTransferStatus.Received),
            () => lot.WarehouseId.ShouldBe(Destination),
            () => lot.SupplierId.ShouldBe(Supplier),
            () => lot.Source.ShouldBe(InventoryLotSource.TransferIn),
            () => lot.RemainingQuantity.Value.ShouldBe(4m),
            () => lot.UnitCost.Amount.ShouldBe(5m),
            () => destinationBalance.QuantityOnHand.ShouldBe(4m));
    }

    [Fact]
    public async Task CancelAsync_Restores_Source_Lots_And_Balance_After_Dispatch()
    {
        var manager = CreateManager();
        var transfer = NewTransfer();
        var sourceBalance = Balance(Source, 10m);
        var lot = SourceLot(10m);
        await manager.DispatchAsync(transfer, [sourceBalance], [lot]);

        await manager.CancelAsync(transfer, [sourceBalance], [lot]);

        transfer.ShouldSatisfyAllConditions(
            () => transfer.Status.ShouldBe(StockTransferStatus.Cancelled),
            () => lot.RemainingQuantity.Value.ShouldBe(10m),
            () => sourceBalance.QuantityOnHand.ShouldBe(10m));
    }

    [Fact]
    public async Task DispatchAsync_On_A_Dispatched_Transfer_Throws_Without_Consuming_Again()
    {
        var manager = CreateManager();
        var transfer = NewTransfer();
        var sourceBalance = Balance(Source, 10m);
        var lot = SourceLot(10m);
        await manager.DispatchAsync(transfer, [sourceBalance], [lot]);

        await Should.ThrowAsync<StockTransferNotInDraftStateDomainException>(
            () => manager.DispatchAsync(transfer, [sourceBalance], [lot]));

        transfer.ShouldSatisfyAllConditions(
            () => transfer.Allocations.Count.ShouldBe(1),
            () => lot.RemainingQuantity.Value.ShouldBe(6m),
            () => sourceBalance.QuantityOnHand.ShouldBe(6m));
    }

    [Fact]
    public async Task ReceiveAsync_Throws_When_Not_Dispatched()
    {
        var manager = CreateManager();
        var transfer = NewTransfer();

        await Should.ThrowAsync<StockTransferNotDispatchedDomainException>(
            () => manager.ReceiveAsync(transfer, [Balance(Destination, 0m)]));
    }

    [Fact]
    public void Creating_A_Transfer_Throws_When_Source_Equals_Destination()
    {
        Should.Throw<StockTransferSameWarehouseDomainException>(
            () => new StockTransfer(Guid.NewGuid(), Source, Source, []));
    }
}
