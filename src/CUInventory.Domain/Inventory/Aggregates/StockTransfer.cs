using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Inventory.Aggregates;

public class StockTransfer : FullAuditedAggregateRoot<Guid>
{
    protected StockTransfer()
    {
    }
}
