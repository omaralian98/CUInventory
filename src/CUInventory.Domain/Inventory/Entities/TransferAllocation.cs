using System;
using CUInventory.Common;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Inventory.Entities;

public class TransferAllocation : FullAuditedEntity<Guid>
{
    public Guid StockTransferId { get; private set; }
    public Guid SourceLotId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public Money UnitCost { get; private set; }
    public Quantity Quantity { get; private set; }

    protected TransferAllocation()
    {
    }

    internal TransferAllocation(
        Guid id,
        Guid stockTransferId,
        Guid sourceLotId,
        Guid productId,
        Guid? supplierId,
        Money unitCost,
        Quantity quantity) : base(id)
    {
        StockTransferId = stockTransferId;
        SourceLotId = sourceLotId;
        ProductId = productId;
        SupplierId = supplierId;
        UnitCost = Guard.NotNull(unitCost, nameof(unitCost));
        Quantity = Guard.NotNull(quantity, nameof(quantity));
    }
}
