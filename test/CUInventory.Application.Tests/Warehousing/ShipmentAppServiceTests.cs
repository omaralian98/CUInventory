using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory;
using CUInventory.Inventory.Dtos;
using CUInventory.Procurement;
using CUInventory.Procurement.Dtos;
using CUInventory.Warehousing.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Warehousing;

public abstract class ShipmentAppServiceTests<TStartupModule> : CUInventoryStockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private async Task<PurchaseOrderDto> CreateConfirmedOrderAsync(
        Guid warehouseId, Guid productId, Guid supplierId, decimal quantity, decimal unitCost)
    {
        var order = await PurchaseOrderAppService.CreateAsync(new CreatePurchaseOrderDto
        {
            SupplierId = supplierId,
            DestinationWarehouseId = warehouseId,
            Lines = { new CreatePurchaseOrderLineDto { ProductId = productId, OrderedQuantity = quantity, UnitCost = unitCost } }
        });
        return await PurchaseOrderAppService.ConfirmAsync(order.Id, await StampOfPurchaseOrderAsync(order.Id));
    }

    [Fact]
    public async Task Should_Create_Dispatch_And_Receive_Updating_Order_Balance_And_Lots()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();

        var order = await CreateConfirmedOrderAsync(warehouseId, productId, supplierId, quantity: 10m, unitCost: 4m);

        var shipment = await ShipmentAppService.CreateAsync(new CreateShipmentDto
        {
            PurchaseOrderId = order.Id,
            SupplierId = supplierId,
            DestinationWarehouseId = warehouseId,
            // Receive only part of the order to prove partial-receipt bookkeeping.
            Lines = { new CreateShipmentLineDto { ProductId = productId, Quantity = 6m, UnitCost = 4m } }
        });
        shipment.Status.ShouldBe(ShipmentStatus.Draft);

        var dispatched = await ShipmentAppService.DispatchAsync(shipment.Id, await StampOfShipmentAsync(shipment.Id));
        dispatched.ShouldSatisfyAllConditions(
            () => dispatched.Status.ShouldBe(ShipmentStatus.Dispatched),
            () => dispatched.DispatchedAt.ShouldNotBeNull());

        var received = await ShipmentAppService.ReceiveAsync(shipment.Id, await StampOfShipmentAsync(shipment.Id));
        received.ShouldSatisfyAllConditions(
            () => received.Status.ShouldBe(ShipmentStatus.Received),
            () => received.ReceivedAt.ShouldNotBeNull());

        var orderAfter = await PurchaseOrderAppService.GetAsync(order.Id);
        orderAfter.ShouldSatisfyAllConditions(
            () => orderAfter.Status.ShouldBe(PurchaseOrderStatus.PartiallyReceived),
            () => orderAfter.Lines.Single().ReceivedQuantity.ShouldBe(6m),
            () => orderAfter.Lines.Single().OutstandingQuantity.ShouldBe(4m));

        var balances = await InventoryBalanceAppService.GetListAsync(
            new GetInventoryBalanceListDto { WarehouseId = warehouseId, ProductId = productId });
        balances.Items.Single().QuantityOnHand.ShouldBe(6m);

        var lots = await InventoryLotAppService.GetListAsync(
            new GetInventoryLotListDto { WarehouseId = warehouseId, ProductId = productId });
        var lot = lots.Items.Single();
        lot.ShouldSatisfyAllConditions(
            () => lot.Source.ShouldBe(InventoryLotSource.Purchase),
            () => lot.OriginalQuantity.ShouldBe(6m),
            () => lot.RemainingQuantity.ShouldBe(6m),
            () => lot.UnitCost.ShouldBe(4m),
            () => lot.SupplierId.ShouldBe(supplierId));
    }

    [Fact]
    public async Task Should_Not_Allow_Receiving_Before_Dispatch()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var order = await CreateConfirmedOrderAsync(warehouseId, productId, supplierId, quantity: 5m, unitCost: 2m);

        var shipment = await ShipmentAppService.CreateAsync(new CreateShipmentDto
        {
            PurchaseOrderId = order.Id,
            SupplierId = supplierId,
            DestinationWarehouseId = warehouseId,
            Lines = { new CreateShipmentLineDto { ProductId = productId, Quantity = 5m, UnitCost = 2m } }
        });

        var stamp = await StampOfShipmentAsync(shipment.Id);
        await Should.ThrowAsync<BusinessException>(() => ShipmentAppService.ReceiveAsync(shipment.Id, stamp));
    }

}
