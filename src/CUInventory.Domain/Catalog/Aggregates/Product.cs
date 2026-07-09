using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Catalog.Aggregates;

public class Product : FullAuditedAggregateRoot<Guid>
{
    protected Product()
    {
    }
}
