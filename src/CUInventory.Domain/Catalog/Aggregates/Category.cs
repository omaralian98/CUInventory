using System;
using CUInventory.Abstractions;
using CUInventory.Common;
using Volo.Abp.Domain.Entities.Auditing;

namespace CUInventory.Catalog.Aggregates;

public class Category : FullAuditedWithIsActiveAndOrderAggregateRoot<Guid>
{
    public string Name { get; private set; }

    protected Category()
    {
    }

    internal Category(Guid id, string name) : base(id)
    {
        SetName(name);
        Activate();
    }

    internal void SetName(string name)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }
}
