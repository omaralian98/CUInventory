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

    private static InventoryLot Lot(decimal quantity = 10m, decimal unitCost = 5m)
        => new(Guid.NewGuid(), Product, Warehouse, InventoryLotSource.Purchase, new Quantity(quantity), new Money(unitCost), Now, Supplier);

    private static SaleLineRequest Line(decimal quantity)
        => new(Product, quantity, 20m, WarehouseId: Warehouse);

    [Fact]
    public async Task CreateAsync_Reserves_Stock_Without_Binding_Lots()
    {
        var manager = CreateManager();
        var balance = Balance(10m);

        var sale = await manager.CreateAsync([Line(4m)], [balance], []);

        sale.ShouldSatisfyAllConditions(
            () => sale.Status.ShouldBe(SaleStatus.Draft),
            () => balance.QuantityReserved.ShouldBe(4m),
            () => balance.QuantityAvailable.ShouldBe(6m),
            () => sale.Lines.Single().Allocations.ShouldAllBe(a => a.IsReserved));
    }

    [Fact]
    public async Task ConfirmAsync_Binds_Lots_Deducts_Stock_And_Raises_SaleCompleted()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var lot = Lot(10m);
        var sale = await manager.CreateAsync([Line(4m)], [balance], []);

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
            () => balance.QuantityOnHand.ShouldBe(6m),
            () => balance.QuantityReserved.ShouldBe(0m),
            () => sale.GetLocalEvents().Select(e => e.EventData).OfType<SaleCompletedDomainEvent>().ShouldHaveSingleItem());
    }

    [Fact]
    public async Task CancelAsync_Releases_The_Reservation()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var sale = await manager.CreateAsync([Line(4m)], [balance], []);

        await manager.CancelAsync(sale, [balance]);

        sale.ShouldSatisfyAllConditions(
            () => sale.Status.ShouldBe(SaleStatus.Cancelled),
            () => balance.QuantityReserved.ShouldBe(0m),
            () => balance.QuantityAvailable.ShouldBe(10m));
    }

    [Fact]
    public async Task CancelAsync_On_A_Cancelled_Sale_Throws_State_Exception()
    {
        var manager = CreateManager();
        var balance = Balance(10m);
        var sale = await manager.CreateAsync([Line(4m)], [balance], []);
        await manager.CancelAsync(sale, [balance]);

        await Should.ThrowAsync<SaleNotInDraftStateDomainException>(
            () => manager.CancelAsync(sale, [balance]));
        balance.QuantityReserved.ShouldBe(0m);
    }

    [Fact]
    public async Task CreateAsync_With_Insufficient_Spread_Reserves_Nothing()
    {
        var manager = CreateManager();
        var first = new InventoryBalance(Guid.NewGuid(), Guid.NewGuid(), Product);
        first.Increase(new Quantity(3m), Now);
        var second = new InventoryBalance(Guid.NewGuid(), Guid.NewGuid(), Product);
        second.Increase(new Quantity(2m), Now);

        await Should.ThrowAsync<InsufficientStockDomainException>(
            () => manager.CreateAsync([new SaleLineRequest(Product, 10m, 20m)], [first, second], []));

        first.ShouldSatisfyAllConditions(
            () => first.QuantityReserved.ShouldBe(0m),
            () => second.QuantityReserved.ShouldBe(0m));
    }

    [Fact]
    public async Task Two_Competing_Sales_Cannot_Oversell_The_Same_Unit()
    {
        var manager = CreateManager();
        var balance = Balance(1m);

        await manager.CreateAsync([Line(1m)], [balance], []);

        await Should.ThrowAsync<InsufficientStockDomainException>(
            () => manager.CreateAsync([Line(1m)], [balance], []));
        balance.QuantityReserved.ShouldBe(1m);
    }

    [Fact]
    public async Task CreateAsync_Reserves_From_The_Warehouse_That_Holds_The_Requested_Supplier()
    {
        var manager = CreateManager();
        var warehouseWithout = Guid.NewGuid();
        var warehouseWith = Guid.NewGuid();
        var otherSupplier = Guid.NewGuid();

        // Higher availability, but stock is from the wrong supplier.
        var balanceWithout = new InventoryBalance(Guid.NewGuid(), warehouseWithout, Product);
        balanceWithout.Increase(new Quantity(10m), Now);
        var lotWithout = new InventoryLot(Guid.NewGuid(), Product, warehouseWithout, InventoryLotSource.Purchase, new Quantity(10m), new Money(5m), Now, otherSupplier);

        // Lower availability, but this is where the requested supplier's stock lives.
        var balanceWith = new InventoryBalance(Guid.NewGuid(), warehouseWith, Product);
        balanceWith.Increase(new Quantity(5m), Now);
        var lotWith = new InventoryLot(Guid.NewGuid(), Product, warehouseWith, InventoryLotSource.Purchase, new Quantity(5m), new Money(5m), Now, Supplier);

        var request = new SaleLineRequest(Product, 5m, 20m, AllocationStrategyKind.SpecificSupplier, SupplierId: Supplier);
        var balances = new List<InventoryBalance> { balanceWithout, balanceWith };
        var lots = new List<InventoryLot> { lotWithout, lotWith };

        var sale = await manager.CreateAsync([request], balances, lots);

        // Reservation skips the higher-availability warehouse because it has no supplier stock.
        sale.ShouldSatisfyAllConditions(
            () => balanceWithout.QuantityReserved.ShouldBe(0m),
            () => balanceWith.QuantityReserved.ShouldBe(5m),
            () => sale.Lines.Single().Allocations.Single().WarehouseId.ShouldBe(warehouseWith));

        // And because reservation was lot-aware, confirmation binds the supplier's lot without throwing.
        await manager.ConfirmAsync(sale, balances, lots);
        sale.Lines.Single().Allocations.Single().InventoryLotId.ShouldBe(lotWith.Id);
    }
}
