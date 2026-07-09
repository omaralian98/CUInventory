using CUInventory.Abstractions;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory;

public class FullAuditedWithIsActiveAndOrderEntity<TKey> : FullAuditedEntity<TKey>, IIsActive, ISortable
{
    public bool IsActive { get; private set; }
    public int OrderIndex { get; set; }

    protected FullAuditedWithIsActiveAndOrderEntity() : base()
    {
    }

    protected FullAuditedWithIsActiveAndOrderEntity(TKey id) : base(id)
    {
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void ToggleIsActive() => IsActive = !IsActive;
    public void SetIsActive(bool isActive) => IsActive = isActive;
}
