using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Sales.Aggregates;

public class Sale : FullAuditedAggregateRoot<Guid>
{
    protected Sale()
    {
    }
}
