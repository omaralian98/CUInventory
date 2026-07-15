using System;
using CUInventory.Abstractions;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CUInventory;

public class FullAuditedWithIsActiveAndOrderAggregateRoot<TKey> : FullAuditedAggregateRoot<TKey>, IIsActive, ISortable, IMultiTenant
{
    public bool IsActive { get; private set; } = true;
    public int OrderIndex { get; set; }
    public Guid? TenantId { get; protected set; }

    protected FullAuditedWithIsActiveAndOrderAggregateRoot() : base()
    {
    }

    protected FullAuditedWithIsActiveAndOrderAggregateRoot(TKey id) : base(id)
    {
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void ToggleIsActive() => IsActive = !IsActive;
    public void SetIsActive(bool isActive) => IsActive = isActive;
}
