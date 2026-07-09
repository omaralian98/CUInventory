using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Procurement.Aggregates;

public class Supplier : FullAuditedAggregateRoot<Guid>
{
    protected Supplier()
    {
    }
}
