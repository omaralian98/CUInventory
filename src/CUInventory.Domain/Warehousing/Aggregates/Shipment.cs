using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Warehousing.Aggregates;

public class Shipment : FullAuditedAggregateRoot<Guid>
{
    protected Shipment()
    {
    }
}
