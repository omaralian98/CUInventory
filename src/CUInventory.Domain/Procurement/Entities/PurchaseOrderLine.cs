using System;
using CUInventory.Common;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Procurement.Entities;

public class PurchaseOrderLine : Entity<Guid>
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity OrderedQuantity { get; private set; }
    public Quantity ReceivedQuantity { get; private set; }
    public Money UnitCost { get; private set; }

    public Quantity OutstandingQuantity => OrderedQuantity.Subtract(ReceivedQuantity);
    public bool IsFullyReceived => OutstandingQuantity.IsZero;

    protected PurchaseOrderLine()
    {
    }

    internal PurchaseOrderLine(Guid id, Guid purchaseOrderId, Guid productId, Quantity orderedQuantity, Money unitCost)
        : base(id)
    {
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        OrderedQuantity = Guard.NotNull(orderedQuantity, nameof(orderedQuantity));
        UnitCost = Guard.NotNull(unitCost, nameof(unitCost));
        ReceivedQuantity = Quantity.Zero;
    }

    internal void AddReceipt(Quantity quantity)
    {
        ReceivedQuantity = ReceivedQuantity.Add(quantity);
    }
}
