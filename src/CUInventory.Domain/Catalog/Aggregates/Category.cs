using System;
using CUInventory.Abstractions;
using Volo.Abp;
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
    }

    internal void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }
}
