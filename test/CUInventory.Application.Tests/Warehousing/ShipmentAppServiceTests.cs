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

    private async Task<ShipmentDto> CreateDraftShipmentAsync(
        Guid orderId, Guid warehouseId, Guid productId, Guid supplierId, decimal quantity, decimal unitCost)
    {
        return await ShipmentAppService.CreateAsync(new CreateShipmentDto
        {
            PurchaseOrderId = orderId,
            SupplierId = supplierId,
            DestinationWarehouseId = warehouseId,
            Lines = { new CreateShipmentLineDto { ProductId = productId, Quantity = quantity, UnitCost = unitCost } }
        });
    }

    [Fact]
    public async Task Should_Create_Dispatch_And_Receive_Updating_Order_Balance_And_Lots()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();

        var order = await CreateConfirmedOrderAsync(warehouseId, productId, supplierId, quantity: 10m, unitCost: 4m);

        // Receive only part of the order to prove partial-receipt bookkeeping.
        var shipment = await CreateDraftShipmentAsync(order.Id, warehouseId, productId, supplierId, quantity: 6m, unitCost: 4m);
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

        var shipment = await CreateDraftShipmentAsync(order.Id, warehouseId, productId, supplierId, quantity: 5m, unitCost: 2m);

        var stamp = await StampOfShipmentAsync(shipment.Id);
        await Should.ThrowAsync<BusinessException>(() => ShipmentAppService.ReceiveAsync(shipment.Id, stamp));
    }

    [Fact]
    public async Task Should_Fully_Receive_Order_Marking_It_Received()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var order = await CreateConfirmedOrderAsync(warehouseId, productId, supplierId, quantity: 8m, unitCost: 3m);

        var shipment = await CreateDraftShipmentAsync(order.Id, warehouseId, productId, supplierId, quantity: 8m, unitCost: 3m);
        await ShipmentAppService.DispatchAsync(shipment.Id, await StampOfShipmentAsync(shipment.Id));
        await ShipmentAppService.ReceiveAsync(shipment.Id, await StampOfShipmentAsync(shipment.Id));

        var orderAfter = await PurchaseOrderAppService.GetAsync(order.Id);
        orderAfter.ShouldSatisfyAllConditions(
            () => orderAfter.Status.ShouldBe(PurchaseOrderStatus.FullyReceived),
            () => orderAfter.Lines.Single().ReceivedQuantity.ShouldBe(8m),
            () => orderAfter.Lines.Single().OutstandingQuantity.ShouldBe(0m));

        var balances = await InventoryBalanceAppService.GetListAsync(
            new GetInventoryBalanceListDto { WarehouseId = warehouseId, ProductId = productId });
        balances.Items.Single().QuantityOnHand.ShouldBe(8m);
    }

    [Fact]
    public async Task Should_Not_Allow_Dispatching_Twice()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var order = await CreateConfirmedOrderAsync(warehouseId, productId, supplierId, quantity: 5m, unitCost: 2m);

        var shipment = await CreateDraftShipmentAsync(order.Id, warehouseId, productId, supplierId, quantity: 5m, unitCost: 2m);
        await ShipmentAppService.DispatchAsync(shipment.Id, await StampOfShipmentAsync(shipment.Id));

        var stamp = await StampOfShipmentAsync(shipment.Id);
        await Should.ThrowAsync<BusinessException>(() => ShipmentAppService.DispatchAsync(shipment.Id, stamp));
    }

    [Fact]
    public async Task Should_Filter_List_By_PurchaseOrder_Warehouse_And_Status()
    {
        var firstWarehouseId = Guid.NewGuid();
        var secondWarehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var firstOrder = await CreateConfirmedOrderAsync(firstWarehouseId, productId, supplierId, quantity: 5m, unitCost: 2m);
        var secondOrder = await CreateConfirmedOrderAsync(secondWarehouseId, productId, supplierId, quantity: 5m, unitCost: 2m);

        var firstShipment = await CreateDraftShipmentAsync(firstOrder.Id, firstWarehouseId, productId, supplierId, quantity: 5m, unitCost: 2m);
        var secondShipment = await CreateDraftShipmentAsync(secondOrder.Id, secondWarehouseId, productId, supplierId, quantity: 5m, unitCost: 2m);
        await ShipmentAppService.DispatchAsync(secondShipment.Id, await StampOfShipmentAsync(secondShipment.Id));

        var byOrder = await ShipmentAppService.GetListAsync(new GetShipmentListDto { PurchaseOrderId = firstOrder.Id });
        byOrder.Items.ShouldHaveSingleItem().Id.ShouldBe(firstShipment.Id);

        var byWarehouse = await ShipmentAppService.GetListAsync(new GetShipmentListDto { DestinationWarehouseId = secondWarehouseId });
        byWarehouse.Items.ShouldHaveSingleItem().Id.ShouldBe(secondShipment.Id);

        var byStatus = await ShipmentAppService.GetListAsync(new GetShipmentListDto { Status = ShipmentStatus.Dispatched });
        byStatus.Items.ShouldContain(s => s.Id == secondShipment.Id);
        byStatus.Items.ShouldNotContain(s => s.Id == firstShipment.Id);
    }
}
