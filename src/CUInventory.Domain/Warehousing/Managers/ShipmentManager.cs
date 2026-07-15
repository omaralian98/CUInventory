using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory;
using CUInventory.Inventory.Aggregates;
using CUInventory.Procurement;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Exceptions;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Exceptions;
using CUInventory.Warehousing.Interfaces;

namespace CUInventory.Warehousing.Managers;

public class ShipmentManager : DomainService, IShipmentManager
{
    public Task<Shipment> CreateAsync(
        PurchaseOrder purchaseOrder,
        Guid supplierId,
        Guid destinationWarehouseId,
        IReadOnlyCollection<ShipmentLineData> lines)
    {
        if (purchaseOrder.Status is not (PurchaseOrderStatus.Confirmed or PurchaseOrderStatus.PartiallyReceived))
        {
            throw new PurchaseOrderNotConfirmedDomainException(purchaseOrder.Id, purchaseOrder.Status);
        }

        if (supplierId != purchaseOrder.SupplierId)
        {
            throw new ShipmentSupplierMismatchDomainException(purchaseOrder.Id, supplierId, purchaseOrder.SupplierId);
        }

        if (destinationWarehouseId != purchaseOrder.DestinationWarehouseId)
        {
            throw new ShipmentWarehouseMismatchDomainException(purchaseOrder.Id, destinationWarehouseId, purchaseOrder.DestinationWarehouseId);
        }

        foreach (var line in lines)
        {
            var orderLine = purchaseOrder.Lines.FirstOrDefault(l => l.ProductId == line.ProductId)
                            ?? throw new PurchaseOrderLineNotFoundDomainException(purchaseOrder.Id, line.ProductId);

            if (line.Quantity > orderLine.OutstandingQuantity)
            {
                throw new ShipmentExceedsOutstandingDomainException(
                    purchaseOrder.Id, line.ProductId, line.Quantity.Value, orderLine.OutstandingQuantity.Value);
            }
        }

        return Task.FromResult(new Shipment(
            GuidGenerator.Create(), purchaseOrder.Id, supplierId, destinationWarehouseId, lines));
    }

    public Task<List<InventoryLot>> ReceiveAsync(
        Shipment shipment,
        PurchaseOrder purchaseOrder,
        List<InventoryBalance> destinationBalances)
    {
        var now = Clock.Now;
        shipment.MarkReceived(now);

        var createdLots = new List<InventoryLot>();

        foreach (var line in shipment.Lines)
        {
            var lot = new InventoryLot(
                GuidGenerator.Create(),
                line.ProductId,
                shipment.DestinationWarehouseId,
                InventoryLotSource.Purchase,
                line.Quantity,
                line.UnitCost,
                now,
                shipment.SupplierId,
                line.Id);

            createdLots.Add(lot);

            var balance = destinationBalances.FindRequired(shipment.DestinationWarehouseId, line.ProductId);
            balance.Increase(line.Quantity, now);

            purchaseOrder.RegisterReceipt(line.ProductId, line.Quantity);
        }

        return Task.FromResult(createdLots);
    }
}
