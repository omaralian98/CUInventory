using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Dtos;
using CUInventory.Inventory.Repositories;
using CUInventory.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Inventory;

[Authorize(CUInventoryPermissions.InventoryBalances.Default)]
public class InventoryBalanceAppService :
    CUInventoryReadOnlyAppService<InventoryBalance, InventoryBalanceDto, InventoryBalanceDto, Guid, GetInventoryBalanceListDto>,
    IInventoryBalanceAppService
{
    private readonly IInventoryBalanceRepository _repository;

    public InventoryBalanceAppService(IInventoryBalanceRepository repository)
        : base(repository)
    {
        _repository = repository;

        GetPolicyName = CUInventoryPermissions.InventoryBalances.Default;
        GetListPolicyName = CUInventoryPermissions.InventoryBalances.Default;
    }

    public virtual async Task<InventoryBalanceDto> SetLowStockThresholdAsync(Guid id, SetLowStockThresholdDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.InventoryBalances.SetThreshold);

        var balance = await _repository.GetAsync(id);
        balance.SetLowStockThreshold(input.Threshold);

        await _repository.UpdateAsync(balance, autoSave: true);
        return await MapToGetOutputDtoAsync(balance);
    }

    protected override async Task<IQueryable<InventoryBalance>> CreateFilteredQueryAsync(GetInventoryBalanceListDto input)
    {
        var query = await _repository.GetQueryableAsync();
        return query
            .WhereIf(input.WarehouseId.HasValue, b => b.WarehouseId == input.WarehouseId!.Value)
            .WhereIf(input.ProductId.HasValue, b => b.ProductId == input.ProductId!.Value)
            .WhereIf(
                input.LowStockOnly == true,
                b => b.LowStockThreshold != null && (b.QuantityOnHand - b.QuantityReserved) < b.LowStockThreshold);
    }
}
