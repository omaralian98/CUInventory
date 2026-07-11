using System;
using CUInventory.Common;
using CUInventory.Inventory.Exceptions;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CUInventory.Inventory.Aggregates;

public class InventoryLot : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid ProductId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid? ShipmentLineId { get; private set; }
    public InventoryLotSource Source { get; private set; }
    public Quantity OriginalQuantity { get; private set; }
    public Quantity RemainingQuantity { get; private set; }
    public Money UnitCost { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public Guid? TenantId { get; protected set; }

    protected InventoryLot()
    {
    }

    public InventoryLot(
        Guid id,
        Guid productId,
        Guid warehouseId,
        InventoryLotSource source,
        Quantity quantity,
        Money unitCost,
        DateTime receivedAt,
        Guid? supplierId = null,
        Guid? shipmentLineId = null) : base(id)
    {
        Guard.Positive(quantity, nameof(quantity));

        ProductId = productId;
        WarehouseId = warehouseId;
        Source = source;
        OriginalQuantity = quantity;
        RemainingQuantity = quantity;
        UnitCost = Guard.NotNull(unitCost, nameof(unitCost));
        ReceivedAt = receivedAt;
        SupplierId = supplierId;
        ShipmentLineId = shipmentLineId;
    }

    internal void Consume(Quantity quantity)
    {
        Guard.Positive(quantity, nameof(quantity));

        if (quantity.Value > RemainingQuantity.Value)
        {
            throw new InventoryLotInsufficientRemainingDomainException(Id, quantity.Value, RemainingQuantity.Value);
        }

        RemainingQuantity = RemainingQuantity.Subtract(quantity);
    }

    internal void Restore(Quantity quantity)
    {
        Guard.Positive(quantity, nameof(quantity));

        if (RemainingQuantity.Value + quantity.Value > OriginalQuantity.Value)
        {
            throw new ArgumentException("Restored quantity exceeds the lot's original quantity.", nameof(quantity));
        }

        RemainingQuantity = RemainingQuantity.Add(quantity);
    }
}
