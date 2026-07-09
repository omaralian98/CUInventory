using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Inventory.Aggregates;

public class InventoryLot : FullAuditedAggregateRoot<Guid>
{
    protected InventoryLot()
    {
    }
}
