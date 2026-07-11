using System;
using CUInventory.Common;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CUInventory.Procurement.Aggregates;

public class Supplier : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string Name { get; private set; }
    public ContactInfo Contact { get; private set; }

    protected Supplier()
    {
    }

    internal Supplier(Guid id, string name, ContactInfo contact) : base(id)
    {
        SetName(name);
        SetContact(contact);
    }

    internal void SetName(string name)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    internal void SetContact(ContactInfo contact)
    {
        Contact = Guard.NotNull(contact, nameof(contact));
    }
}
