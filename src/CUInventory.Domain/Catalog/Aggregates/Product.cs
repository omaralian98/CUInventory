using System;
using CUInventory.ValueObjects;
using Volo.Abp;

namespace CUInventory.Catalog.Aggregates;


public class Product : FullAuditedWithIsActiveAndOrderAggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Sku? SKU { get; private set; }
    public bool IsService { get; private set; }
    public Guid? CategoryId { get; private set; }


    protected Product()
    {
    }

    internal Product(
        Guid id,
        string name,
        string? description,
        Sku? sku,
        bool isService = false,
        Guid? categoryId = null) : base(id)
    {
        SetName(name);
        SetDescription(description);
        SetSku(sku);
        SetIsService(isService);
        SetCategory(categoryId);
    }


    public void SetName(string name)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));

        Name = name;
    }


    internal void SetSku(Sku? sku)
    {
        SKU = sku;
    }

    public void SetDescription(string? description)
    {
        Description = description;
    }

    public void SetCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
    }

    public void SetIsService(bool isService)
    {
        IsService = isService;
    }
}
