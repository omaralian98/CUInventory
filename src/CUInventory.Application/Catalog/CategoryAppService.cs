using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Dtos;
using CUInventory.Catalog.Interfaces;
using CUInventory.Catalog.Repositories;
using CUInventory.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Catalog;

[Authorize(CUInventoryPermissions.Categories.Default)]
public class CategoryAppService :
    CUInventoryCrudAppService<Category, CategoryDto, CategoryDto, Guid, GetCategoryListDto, CreateCategoryDto, UpdateCategoryDto>,
    ICategoryAppService
{
    private readonly ICategoryManager _categoryManager;

    public CategoryAppService(ICategoryRepository repository, ICategoryManager categoryManager)
        : base(repository)
    {
        _categoryManager = categoryManager;

        GetPolicyName = CUInventoryPermissions.Categories.Default;
        GetListPolicyName = CUInventoryPermissions.Categories.Default;
        CreatePolicyName = CUInventoryPermissions.Categories.Create;
        UpdatePolicyName = CUInventoryPermissions.Categories.Edit;
        DeletePolicyName = CUInventoryPermissions.Categories.Delete;
    }

    public override async Task<CategoryDto> CreateAsync(CreateCategoryDto input)
    {
        await CheckCreatePolicyAsync();

        var category = await _categoryManager.CreateAsync(input.Name);
        category.OrderIndex = input.OrderIndex;
        category.SetIsActive(input.IsActive);

        await Repository.InsertAsync(category);
        return await MapToGetOutputDtoAsync(category);
    }

    public override async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryDto input)
    {
        await CheckUpdatePolicyAsync();

        var category = await Repository.GetAsync(id);
        category.ConcurrencyStamp = input.ConcurrencyStamp;
        await _categoryManager.UpdateAsync(category, input.Name, input.OrderIndex, input.IsActive);

        await Repository.UpdateAsync(category, autoSave: true);
        return await MapToGetOutputDtoAsync(category);
    }

    protected override async Task<IQueryable<Category>> CreateFilteredQueryAsync(GetCategoryListDto input)
    {
        var query = await Repository.GetQueryableAsync();
        return query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), c => c.Name.Contains(input.Filter!));
    }
}
