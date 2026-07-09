using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Inventory.Aggregates;

public class InventoryBalance : FullAuditedAggregateRoot<Guid>
{
    protected InventoryBalance()
    {
    }
}
