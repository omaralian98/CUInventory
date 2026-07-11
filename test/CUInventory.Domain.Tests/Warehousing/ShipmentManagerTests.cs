using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory;
using CUInventory.Inventory.Aggregates;
using CUInventory.Procurement;
using CUInventory.Procurement.Aggregates;
using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Exceptions;
using CUInventory.Warehousing.Managers;
using Shouldly;
using Xunit;

namespace CUInventory.Warehousing;

public class ShipmentManagerTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;
    private static readonly Guid Product = Guid.NewGuid();

    private static ShipmentManager CreateManager()
        => new ShipmentManager().WithTestGuidGenerator();

    private static PurchaseOrder ConfirmedOrder()
    {
        var order = new PurchaseOrder(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            [new PurchaseOrderLineData(Guid.NewGuid(), Product, new Quantity(10m), new Money(5m))]);
        order.Confirm(Now);
        return order;
    }

    private static Shipment DispatchedShipment(PurchaseOrder order)
    {
        var shipment = new Shipment(
            Guid.NewGuid(),
            order.Id,
            order.SupplierId,
            order.DestinationWarehouseId,
            [new ShipmentLineData(Guid.NewGuid(), Product, new Quantity(10m), new Money(5m))]);
        shipment.Dispatch(Now);
        return shipment;
    }

    private static InventoryBalance Balance(Guid warehouseId)
        => new(Guid.NewGuid(), warehouseId, Product);

    [Fact]
    public async Task ReceiveAsync_Creates_Lots_Increments_Balance_And_Updates_The_Order()
    {
        var manager = CreateManager();
        var order = ConfirmedOrder();
        var shipment = DispatchedShipment(order);
        var balance = Balance(order.DestinationWarehouseId);

        var lots = await manager.ReceiveAsync(shipment, order, [balance]);

        var lot = lots.ShouldHaveSingleItem();
        lot.ShouldSatisfyAllConditions(
            () => lot.ProductId.ShouldBe(Product),
            () => lot.WarehouseId.ShouldBe(order.DestinationWarehouseId),
            () => lot.SupplierId.ShouldBe(order.SupplierId),
            () => lot.Source.ShouldBe(InventoryLotSource.Purchase),
            () => lot.RemainingQuantity.Value.ShouldBe(10m),
            () => lot.ShipmentLineId.ShouldBe(shipment.Lines.Single().Id),
            () => balance.QuantityOnHand.ShouldBe(10m),
            () => order.Status.ShouldBe(PurchaseOrderStatus.FullyReceived));
    }

    [Fact]
    public async Task ReceiveAsync_On_A_Received_Shipment_Throws_Without_Duplicating_Stock()
    {
        var manager = CreateManager();
        var order = ConfirmedOrder();
        var shipment = DispatchedShipment(order);
        var balance = Balance(order.DestinationWarehouseId);
        await manager.ReceiveAsync(shipment, order, [balance]);

        await Should.ThrowAsync<ShipmentNotDispatchedDomainException>(
            () => manager.ReceiveAsync(shipment, order, [balance]));
        balance.QuantityOnHand.ShouldBe(10m);
    }
}
