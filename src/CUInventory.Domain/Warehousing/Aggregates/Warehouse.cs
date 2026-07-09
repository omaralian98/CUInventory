using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Warehousing.Aggregates;

public class Warehouse : FullAuditedAggregateRoot<Guid>
{
    protected Warehouse()
    {
    }
}
