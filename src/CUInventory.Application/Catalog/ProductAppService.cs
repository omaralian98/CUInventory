using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Dtos;
using CUInventory.Catalog.Interfaces;
using CUInventory.Catalog.Repositories;
using CUInventory.Catalog.ValueObjects;
using CUInventory.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Catalog;

[Authorize(CUInventoryPermissions.Products.Default)]
public class ProductAppService :
    CUInventoryCrudAppService<Product, ProductDto, ProductDto, Guid, GetProductListDto, CreateProductDto, UpdateProductDto>,
    IProductAppService
{
    private readonly IProductManager _productManager;

    public ProductAppService(IProductRepository repository, IProductManager productManager)
        : base(repository)
    {
        _productManager = productManager;

        GetPolicyName = CUInventoryPermissions.Products.Default;
        GetListPolicyName = CUInventoryPermissions.Products.Default;
        CreatePolicyName = CUInventoryPermissions.Products.Create;
        UpdatePolicyName = CUInventoryPermissions.Products.Edit;
        DeletePolicyName = CUInventoryPermissions.Products.Delete;
    }

    public override async Task<ProductDto> CreateAsync(CreateProductDto input)
    {
        await CheckCreatePolicyAsync();

        var sku = string.IsNullOrWhiteSpace(input.Sku) ? null : new Sku(input.Sku);
        var product = await _productManager.CreateAsync(input.Name, input.Description, sku, input.IsService, input.CategoryId);
        product.OrderIndex = input.OrderIndex;
        product.SetIsActive(input.IsActive);

        await Repository.InsertAsync(product, autoSave: true);
        return await MapToGetOutputDtoAsync(product);
    }

    public override async Task<ProductDto> UpdateAsync(Guid id, UpdateProductDto input)
    {
        await CheckUpdatePolicyAsync();

        var product = await Repository.GetAsync(id);
        var sku = string.IsNullOrWhiteSpace(input.Sku) ? null : new Sku(input.Sku);
        await _productManager.UpdateAsync(product, input.Name, input.Description, sku, input.IsService, input.CategoryId);
        product.OrderIndex = input.OrderIndex;
        product.SetIsActive(input.IsActive);

        await Repository.UpdateAsync(product, autoSave: true);
        return await MapToGetOutputDtoAsync(product);
    }

    protected override async Task<IQueryable<Product>> CreateFilteredQueryAsync(GetProductListDto input)
    {
        var query = await Repository.GetQueryableAsync();
        return query
            .WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                p => p.Name.Contains(input.Filter!) || (p.SKU != null && p.SKU.Value.Contains(input.Filter!)))
            .WhereIf(input.IsActive.HasValue, p => p.IsActive == input.IsActive!.Value)
            .WhereIf(input.CategoryId.HasValue, p => p.CategoryId == input.CategoryId!.Value);
    }
}
