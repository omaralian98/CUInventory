using System;
using CUInventory.Common;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Warehousing.Entities;

public class ShipmentLine : FullAuditedEntity<Guid>
{
    public Guid ShipmentId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money UnitCost { get; private set; }

    protected ShipmentLine()
    {
    }

    internal ShipmentLine(Guid id, Guid shipmentId, Guid productId, Quantity quantity, Money unitCost) : base(id)
    {
        ShipmentId = shipmentId;
        ProductId = productId;
        Quantity = Guard.NotNull(quantity, nameof(quantity));
        UnitCost = Guard.NotNull(unitCost, nameof(unitCost));
    }
}
