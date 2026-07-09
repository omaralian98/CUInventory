using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Procurement.Aggregates;

public class PurchaseOrder : FullAuditedAggregateRoot<Guid>
{
    protected PurchaseOrder()
    {
    }
}
