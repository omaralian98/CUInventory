using System;
using CUInventory.Common;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Inventory.Entities;

public class StockTransferLine : FullAuditedEntity<Guid>
{
    public Guid StockTransferId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity Quantity { get; private set; }

    protected StockTransferLine()
    {
    }

    internal StockTransferLine(Guid id, Guid stockTransferId, Guid productId, Quantity quantity) : base(id)
    {
        StockTransferId = stockTransferId;
        ProductId = productId;
        Quantity = Guard.NotNull(quantity, nameof(quantity));
    }
}
