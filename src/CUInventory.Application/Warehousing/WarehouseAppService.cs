using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Permissions;
using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Dtos;
using CUInventory.Warehousing.Interfaces;
using CUInventory.Warehousing.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Warehousing;

[Authorize(CUInventoryPermissions.Warehouses.Default)]
public class WarehouseAppService :
    CUInventoryCrudAppService<Warehouse, WarehouseDto, WarehouseDto, Guid, GetWarehouseListDto, CreateWarehouseDto, UpdateWarehouseDto>,
    IWarehouseAppService
{
    private readonly IWarehouseManager _warehouseManager;

    public WarehouseAppService(IWarehouseRepository repository, IWarehouseManager warehouseManager)
        : base(repository)
    {
        _warehouseManager = warehouseManager;

        GetPolicyName = CUInventoryPermissions.Warehouses.Default;
        GetListPolicyName = CUInventoryPermissions.Warehouses.Default;
        CreatePolicyName = CUInventoryPermissions.Warehouses.Create;
        UpdatePolicyName = CUInventoryPermissions.Warehouses.Edit;
        DeletePolicyName = CUInventoryPermissions.Warehouses.Delete;
    }

    public override async Task<WarehouseDto> CreateAsync(CreateWarehouseDto input)
    {
        await CheckCreatePolicyAsync();

        var warehouse = await _warehouseManager.CreateAsync(
            input.Name, input.Code, new Address(input.Address.Governorate, input.Address.City, input.Address.Street));
        warehouse.OrderIndex = input.OrderIndex;
        warehouse.SetIsActive(input.IsActive);

        await Repository.InsertAsync(warehouse, autoSave: true);
        return await MapToGetOutputDtoAsync(warehouse);
    }

    public override async Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto input)
    {
        await CheckUpdatePolicyAsync();

        var warehouse = await Repository.GetAsync(id);
        await _warehouseManager.UpdateAsync(
            warehouse, input.Name, input.Code,
            new Address(input.Address.Governorate, input.Address.City, input.Address.Street), input.IsActive);
        warehouse.OrderIndex = input.OrderIndex;

        await Repository.UpdateAsync(warehouse, autoSave: true);
        return await MapToGetOutputDtoAsync(warehouse);
    }

    protected override async Task<IQueryable<Warehouse>> CreateFilteredQueryAsync(GetWarehouseListDto input)
    {
        var query = await Repository.GetQueryableAsync();
        return query
            .WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                w => w.Name.Contains(input.Filter!) || w.Code.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, w => w.IsActive == input.IsActive!.Value);
    }
}
