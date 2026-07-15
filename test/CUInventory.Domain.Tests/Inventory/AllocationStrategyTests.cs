using System;
using System.Collections.Generic;
using System.Linq;
using CUInventory.Common.Exceptions;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Allocation;
using CUInventory.Inventory.Exceptions;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Inventory;

public class AllocationStrategyTests
{
    private static readonly Guid Product = Guid.NewGuid();
    private static readonly DateTime Early = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Late = new(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

    private static InventoryLot Lot(
        decimal quantity,
        DateTime receivedAt,
        Guid? supplierId = null,
        Guid? warehouseId = null,
        decimal unitCost = 5m)
        => new(Guid.NewGuid(), Product, warehouseId ?? Guid.NewGuid(), InventoryLotSource.Purchase, new Quantity(quantity), new Money(unitCost), receivedAt, supplierId ?? Guid.NewGuid());

    [Fact]
    public void Fifo_Consumes_Oldest_Lots_First_And_Splits()
    {
        var oldest = Lot(6m, Early);
        var newest = Lot(10m, Late);
        var strategy = new FifoAllocationStrategy();

        var results = strategy.Allocate(new AllocationRequest(Product, 8m, AllocationStrategyKind.Fifo), [newest, oldest]);

        results.ShouldSatisfyAllConditions(
            () => results.Count.ShouldBe(2),
            () => results[0].LotId.ShouldBe(oldest.Id),
            () => results[0].Quantity.ShouldBe(6m),
            () => results[1].LotId.ShouldBe(newest.Id),
            () => results[1].Quantity.ShouldBe(2m));
    }

    [Fact]
    public void Fifo_Throws_When_Total_Stock_Is_Insufficient()
    {
        var strategy = new FifoAllocationStrategy();

        Should.Throw<InsufficientStockDomainException>(
            () => strategy.Allocate(new AllocationRequest(Product, 20m, AllocationStrategyKind.Fifo), [Lot(5m, Early)]));
    }

    [Fact]
    public void Fifo_Insufficient_Reports_Requested_And_Available_Quantities()
    {
        var strategy = new FifoAllocationStrategy();

        var ex = Should.Throw<InsufficientStockDomainException>(
            () => strategy.Allocate(new AllocationRequest(Product, 20m, AllocationStrategyKind.Fifo), [Lot(5m, Early)]));

        ex.ShouldSatisfyAllConditions(
            () => ex.Data["Requested"].ShouldBe(20m),
            () => ex.Data["Available"].ShouldBe(5m));
    }

    [Fact]
    public void Fifo_Breaks_Ties_On_Id_When_ReceivedAt_Matches()
    {
        // Both lots share a receipt timestamp, so ordering must fall back to Id for deterministic allocation.
        var a = Lot(5m, Early);
        var b = Lot(5m, Early);
        var strategy = new FifoAllocationStrategy();
        var expectedFirst = new[] { a, b }.OrderBy(l => l.Id).First();

        var results = strategy.Allocate(new AllocationRequest(Product, 5m, AllocationStrategyKind.Fifo), [b, a]);

        results.ShouldHaveSingleItem();
        results[0].LotId.ShouldBe(expectedFirst.Id);
    }

    [Fact]
    public void Allocate_Offers_Only_Unreserved_Quantity()
    {
        var partiallyReserved = Lot(10m, Early);
        partiallyReserved.Reserve(new Quantity(6m));
        var newest = Lot(10m, Late);
        var strategy = new FifoAllocationStrategy();

        var results = strategy.Allocate(new AllocationRequest(Product, 8m, AllocationStrategyKind.Fifo), [newest, partiallyReserved]);

        results.ShouldSatisfyAllConditions(
            () => results.Count.ShouldBe(2),
            () => results[0].LotId.ShouldBe(partiallyReserved.Id),
            () => results[0].Quantity.ShouldBe(4m),
            () => results[1].LotId.ShouldBe(newest.Id),
            () => results[1].Quantity.ShouldBe(4m));
    }

    [Fact]
    public void Allocate_Skips_Fully_Reserved_Lots()
    {
        var fullyReserved = Lot(10m, Early);
        fullyReserved.Reserve(new Quantity(10m));
        var strategy = new FifoAllocationStrategy();

        var ex = Should.Throw<InsufficientStockDomainException>(
            () => strategy.Allocate(new AllocationRequest(Product, 1m, AllocationStrategyKind.Fifo), [fullyReserved]));
        ex.Data["Available"].ShouldBe(0m);
    }

    [Fact]
    public void SpecificLot_Uses_Only_The_Requested_Lots()
    {
        var chosen = Lot(10m, Late);
        var other = Lot(10m, Early);
        var strategy = new SpecificLotAllocationStrategy();

        var results = strategy.Allocate(
            new AllocationRequest(Product, 5m, AllocationStrategyKind.SpecificLot, LotIds: [chosen.Id]),
            [chosen, other]);

        results.ShouldHaveSingleItem();
        results[0].LotId.ShouldBe(chosen.Id);
    }

    [Fact]
    public void SpecificLot_Throws_When_No_Lot_Id_Is_Provided()
    {
        var strategy = new SpecificLotAllocationStrategy();

        Should.Throw<RequiredArgumentDomainException>(
            () => strategy.Allocate(new AllocationRequest(Product, 5m, AllocationStrategyKind.SpecificLot), [Lot(10m, Early)]));
    }

    [Fact]
    public void SpecificSupplier_Filters_By_Supplier()
    {
        var supplier = Guid.NewGuid();
        var fromSupplier = Lot(10m, Late, supplierId: supplier);
        var fromOther = Lot(10m, Early, supplierId: Guid.NewGuid());
        var strategy = new SpecificSupplierAllocationStrategy();

        var results = strategy.Allocate(
            new AllocationRequest(Product, 5m, AllocationStrategyKind.SpecificSupplier, SupplierId: supplier),
            [fromSupplier, fromOther]);

        results.ShouldHaveSingleItem();
        results[0].SupplierId.ShouldBe(supplier);
    }

    [Fact]
    public void SpecificWarehouse_Filters_By_Warehouse()
    {
        var warehouse = Guid.NewGuid();
        var inWarehouse = Lot(10m, Late, warehouseId: warehouse);
        var elsewhere = Lot(10m, Early, warehouseId: Guid.NewGuid());
        var strategy = new SpecificWarehouseAllocationStrategy();

        var results = strategy.Allocate(
            new AllocationRequest(Product, 5m, AllocationStrategyKind.SpecificWarehouse, WarehouseIds: [warehouse]),
            [inWarehouse, elsewhere]);

        results.ShouldHaveSingleItem();
        results[0].WarehouseId.ShouldBe(warehouse);
    }

    [Fact]
    public void Service_Selects_The_Strategy_Matching_The_Request_Kind()
    {
        var supplier = Guid.NewGuid();
        var fromSupplier = Lot(10m, Late, supplierId: supplier);
        var fromOther = Lot(10m, Early, supplierId: Guid.NewGuid());
        var service = new InventoryAllocationService(AllStrategies());

        var results = service.Allocate(
            new AllocationRequest(Product, 5m, AllocationStrategyKind.SpecificSupplier, SupplierId: supplier),
            [fromSupplier, fromOther]);

        results.ShouldHaveSingleItem();
        results[0].SupplierId.ShouldBe(supplier);
    }

    private static IEnumerable<IInventoryAllocationStrategy> AllStrategies() =>
    [
        new FifoAllocationStrategy(),
        new SpecificLotAllocationStrategy(),
        new SpecificSupplierAllocationStrategy(),
        new SpecificWarehouseAllocationStrategy()
    ];
}
