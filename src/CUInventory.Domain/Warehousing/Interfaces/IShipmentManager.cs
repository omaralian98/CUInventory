using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;
using CUInventory.Procurement.Aggregates;
using CUInventory.Warehousing.Aggregates;

namespace CUInventory.Warehousing.Interfaces;

public interface IShipmentManager : IDomainService
{
    Task<List<InventoryLot>> ReceiveAsync(Shipment shipment, PurchaseOrder purchaseOrder, List<InventoryBalance> destinationBalances);
}
