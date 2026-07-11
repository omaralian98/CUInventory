using System;
using CUInventory.Common;
using CUInventory.ValueObjects;

namespace CUInventory.Warehousing.Aggregates;

public class Warehouse : FullAuditedWithIsActiveAndOrderAggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public Address Address { get; private set; }

    protected Warehouse()
    {
    }

    internal Warehouse(Guid id, string name, string code, Address address) : base(id)
    {
        SetName(name);
        SetCode(code);
        SetAddress(address);
        Activate();
    }

    public void SetName(string name)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    internal void SetCode(string code)
    {
        Guard.NotNullOrWhiteSpace(code, nameof(code));
        Code = NormalizeCode(code);
    }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    public void SetAddress(Address address)
    {
        Address = Guard.NotNull(address, nameof(address));
    }
}
