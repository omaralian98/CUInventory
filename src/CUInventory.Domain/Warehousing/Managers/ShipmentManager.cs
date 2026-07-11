using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory;
using CUInventory.Inventory.Aggregates;
using CUInventory.Procurement.Aggregates;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Interfaces;

namespace CUInventory.Warehousing.Managers;

public class ShipmentManager : DomainService, IShipmentManager
{
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
