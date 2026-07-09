using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Inventory.Aggregates;

public class InventoryAdjustment : FullAuditedAggregateRoot<Guid>
{
    protected InventoryAdjustment()
    {
    }
}
