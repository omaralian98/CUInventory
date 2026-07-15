using System;
using CUInventory.Common;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Sales.Entities;

public class SaleAllocation : FullAuditedEntity<Guid>
{
    public Guid SaleLineId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? InventoryLotId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money? UnitCost { get; private set; }
    public bool IsReserved { get; private set; }

    protected SaleAllocation()
    {
    }

    internal SaleAllocation(
        Guid id,
        Guid saleLineId,
        Guid warehouseId,
        Guid inventoryLotId,
        Guid? supplierId,
        Money unitCost,
        Quantity quantity) : base(id)
    {
        SaleLineId = saleLineId;
        WarehouseId = warehouseId;
        InventoryLotId = inventoryLotId;
        SupplierId = supplierId;
        UnitCost = Guard.NotNull(unitCost, nameof(unitCost));
        Quantity = Guard.NotNull(quantity, nameof(quantity));
        IsReserved = true;
    }

    internal void MarkConfirmed()
    {
        IsReserved = false;
    }
}
