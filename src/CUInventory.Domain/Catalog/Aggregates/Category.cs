using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Catalog.Aggregates;

public class Category : FullAuditedAggregateRoot<Guid>
{
    protected Category()
    {
    }
}
