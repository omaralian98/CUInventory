using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Allocation;
using CUInventory.Inventory.Exceptions;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Events;
using CUInventory.Sales.Exceptions;
using CUInventory.Sales.Managers;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Sales;

public class SaleManagerTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;
    private static readonly Guid Warehouse = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();
    private static readonly Guid Supplier = Guid.NewGuid();

    private static SaleManager CreateManager()
    {
        var allocation = new InventoryAllocationService(
        [
            new FifoAllocationStrategy(),
            new SpecificLotAllocationStrategy(),
            new SpecificSupplierAllocationStrategy(),
            new SpecificWarehouseAllocationStrategy()
        ]);
        return new SaleManager(allocation).WithTestGuidGenerator();
    }

    private static InventoryBalance Balance(decimal onHand)
    {
        var balance = new InventoryBalance(Guid.NewGuid(), Warehouse, Product);
        if (onHand > 0)
        {
            balance.Increase(new Quantity(onHand), Now);
        }

        return balance;
    }

    private static InventoryLot Lot(decimal quantity = 10m, decimal unitCost = 5m, DateTime? receivedAt = null)
        => new(Guid.NewGuid(), Product, Warehouse, InventoryLotSource.Purchase, new Quantity(quantity), new Money(unitCost), receivedAt ?? Now, Supplier);

    private static SaleLineRequest Line(decimal quantity)
        => new(Product, quantity, 20m, WarehouseId: Warehouse);

    [Fact]
    public async Task CreateAsync_Pins_Lots_At_Reservation()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var lot = Lot(10m);

        var sale = await manager.CreateAsync([Line(4m)], [balance], [lot]);

        var allocation = sale.Lines.Single().Allocations.Single();
        sale.ShouldSatisfyAllConditions(
            () => sale.Status.ShouldBe(SaleStatus.Draft),
            () => balance.QuantityReserved.Value.ShouldBe(4m),
            () => balance.QuantityAvailable.ShouldBe(6m),
            () => allocation.IsReserved.ShouldBeTrue(),
            () => allocation.InventoryLotId.ShouldBe(lot.Id),
            () => allocation.SupplierId.ShouldBe(Supplier),
            () => allocation.UnitCost!.Amount.ShouldBe(5m),
            () => lot.ReservedQuantity.Value.ShouldBe(4m),
            () => lot.RemainingQuantity.Value.ShouldBe(10m));
    }

    [Fact]
    public async Task ConfirmAsync_Consumes_The_Pinned_Lots_And_Raises_SaleCompleted()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var lot = Lot(10m);
        var sale = await manager.CreateAsync([Line(4m)], [balance], [lot]);

        await manager.ConfirmAsync(sale, [balance], [lot]);

        var allocation = sale.Lines.Single().Allocations.Single();
        sale.ShouldSatisfyAllConditions(
            () => sale.Status.ShouldBe(SaleStatus.Confirmed),
            () => allocation.IsReserved.ShouldBeFalse(),
            () => allocation.InventoryLotId.ShouldBe(lot.Id),
            () => allocation.SupplierId.ShouldBe(Supplier),
            () => allocation.UnitCost!.Amount.ShouldBe(5m),
            () => allocation.Quantity.Value.ShouldBe(4m),
            () => lot.RemainingQuantity.Value.ShouldBe(6m),
            () => lot.ReservedQuantity.Value.ShouldBe(0m),
            () => balance.QuantityOnHand.Value.ShouldBe(6m),
            () => balance.QuantityReserved.Value.ShouldBe(0m),
            () => sale.GetLocalEvents().Select(e => e.EventData).OfType<SaleCompletedDomainEvent>().ShouldHaveSingleItem());
    }

    [Fact]
    public async Task ConfirmAsync_Consumes_Exactly_The_Pinned_Lots()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var pinned = Lot(10m);
        var sale = await manager.CreateAsync([Line(4m)], [balance], [pinned]);
        var olderArrival = Lot(10m, unitCost: 1m, receivedAt: Now.AddYears(-1));

        await manager.ConfirmAsync(sale, [balance], [pinned, olderArrival]);

        sale.ShouldSatisfyAllConditions(
            () => sale.Lines.Single().Allocations.Single().InventoryLotId.ShouldBe(pinned.Id),
            () => pinned.RemainingQuantity.Value.ShouldBe(6m),
            () => olderArrival.RemainingQuantity.Value.ShouldBe(10m));
    }

    [Fact]
    public async Task CancelAsync_Releases_The_Balance_And_Lot_Reservations()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var lot = Lot(10m);
        var sale = await manager.CreateAsync([Line(4m)], [balance], [lot]);

        await manager.CancelAsync(sale, [balance], [lot]);

        sale.ShouldSatisfyAllConditions(
            () => sale.Status.ShouldBe(SaleStatus.Cancelled),
            () => balance.QuantityReserved.Value.ShouldBe(0m),
            () => balance.QuantityAvailable.ShouldBe(10m),
            () => lot.ReservedQuantity.Value.ShouldBe(0m),
            () => lot.AvailableQuantity.ShouldBe(10m));
    }

    [Fact]
    public async Task CancelAsync_On_A_Cancelled_Sale_Throws_State_Exception()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var lot = Lot(10m);
        var sale = await manager.CreateAsync([Line(4m)], [balance], [lot]);
        await manager.CancelAsync(sale, [balance], [lot]);

        await Should.ThrowAsync<SaleNotInDraftStateDomainException>(
            () => manager.CancelAsync(sale, [balance], [lot]));
        balance.QuantityReserved.Value.ShouldBe(0m);
    }

    [Fact]
    public async Task CreateAsync_With_Insufficient_Spread_Reserves_Nothing()
    {
        var manager = CreateManager();
        var firstWarehouse = Guid.NewGuid();
        var secondWarehouse = Guid.NewGuid();
        var first = new InventoryBalance(Guid.NewGuid(), firstWarehouse, Product);
        first.Increase(new Quantity(3m), Now);
        var second = new InventoryBalance(Guid.NewGuid(), secondWarehouse, Product);
        second.Increase(new Quantity(2m), Now);
        var firstLot = new InventoryLot(Guid.NewGuid(), Product, firstWarehouse, InventoryLotSource.Purchase, new Quantity(3m), new Money(5m), Now, Supplier);
        var secondLot = new InventoryLot(Guid.NewGuid(), Product, secondWarehouse, InventoryLotSource.Purchase, new Quantity(2m), new Money(5m), Now, Supplier);

        await Should.ThrowAsync<InsufficientStockDomainException>(
            () => manager.CreateAsync([new SaleLineRequest(Product, 10m, 20m)], [first, second], [firstLot, secondLot]));

        first.ShouldSatisfyAllConditions(
            () => first.QuantityReserved.Value.ShouldBe(0m),
            () => second.QuantityReserved.Value.ShouldBe(0m));
    }

    [Fact]
    public async Task Two_Competing_Sales_Cannot_Oversell_The_Same_Unit()
    {
        var manager = CreateManager();
        var balance = Balance(1m);
        var lot = Lot(1m);

        await manager.CreateAsync([Line(1m)], [balance], [lot]);

        await Should.ThrowAsync<InsufficientStockDomainException>(
            () => manager.CreateAsync([Line(1m)], [balance], [lot]));
        balance.QuantityReserved.Value.ShouldBe(1m);
    }

    [Fact]
    public async Task CreateAsync_Rejects_Double_Pledge_Of_The_Same_Lot()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var lot = Lot(5m);
        var otherLot = Lot(5m);
        var request = new SaleLineRequest(Product, 3m, 20m, AllocationStrategyKind.SpecificLot, WarehouseId: Warehouse, LotId: lot.Id);

        await Should.ThrowAsync<InsufficientStockDomainException>(
            () => manager.CreateAsync([request, request], [balance], [lot, otherLot]));
    }

    [Fact]
    public async Task Fifo_Sale_Cannot_Steal_A_Pledged_Lot()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var oldest = Lot(5m, receivedAt: Now.AddMonths(-2));
        var newest = Lot(5m);
        var pledgeRequest = new SaleLineRequest(Product, 5m, 20m, AllocationStrategyKind.SpecificLot, WarehouseId: Warehouse, LotId: oldest.Id);

        var pledgedSale = await manager.CreateAsync([pledgeRequest], [balance], [oldest, newest]);
        var fifoSale = await manager.CreateAsync([Line(5m)], [balance], [oldest, newest]);

        fifoSale.Lines.Single().Allocations.Single().InventoryLotId.ShouldBe(newest.Id);

        await manager.ConfirmAsync(fifoSale, [balance], [oldest, newest]);
        await manager.ConfirmAsync(pledgedSale, [balance], [oldest, newest]);

        balance.ShouldSatisfyAllConditions(
            () => balance.QuantityOnHand.Value.ShouldBe(0m),
            () => oldest.RemainingQuantity.Value.ShouldBe(0m),
            () => newest.RemainingQuantity.Value.ShouldBe(0m));
    }

    [Fact]
    public async Task CreateAsync_Reserves_From_The_Warehouse_That_Holds_The_Requested_Supplier()
    {
        var manager = CreateManager();
        var warehouseWithout = Guid.NewGuid();
        var warehouseWith = Guid.NewGuid();
        var otherSupplier = Guid.NewGuid();

        var balanceWithout = new InventoryBalance(Guid.NewGuid(), warehouseWithout, Product);
        balanceWithout.Increase(new Quantity(10m), Now);
        var lotWithout = new InventoryLot(Guid.NewGuid(), Product, warehouseWithout, InventoryLotSource.Purchase, new Quantity(10m), new Money(5m), Now, otherSupplier);

        var balanceWith = new InventoryBalance(Guid.NewGuid(), warehouseWith, Product);
        balanceWith.Increase(new Quantity(5m), Now);
        var lotWith = new InventoryLot(Guid.NewGuid(), Product, warehouseWith, InventoryLotSource.Purchase, new Quantity(5m), new Money(5m), Now, Supplier);

        var request = new SaleLineRequest(Product, 5m, 20m, AllocationStrategyKind.SpecificSupplier, SupplierId: Supplier);
        var balances = new List<InventoryBalance> { balanceWithout, balanceWith };
        var lots = new List<InventoryLot> { lotWithout, lotWith };

        var sale = await manager.CreateAsync([request], balances, lots);

        sale.ShouldSatisfyAllConditions(
            () => balanceWithout.QuantityReserved.Value.ShouldBe(0m),
            () => balanceWith.QuantityReserved.Value.ShouldBe(5m),
            () => sale.Lines.Single().Allocations.Single().WarehouseId.ShouldBe(warehouseWith),
            () => sale.Lines.Single().Allocations.Single().InventoryLotId.ShouldBe(lotWith.Id));

        await manager.ConfirmAsync(sale, balances, lots);
        sale.Lines.Single().Allocations.Single().InventoryLotId.ShouldBe(lotWith.Id);
    }
}
