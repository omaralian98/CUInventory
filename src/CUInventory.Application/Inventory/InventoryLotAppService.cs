using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Dtos;
using CUInventory.Inventory.Repositories;
using CUInventory.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Inventory;

[Authorize(CUInventoryPermissions.InventoryLots.Default)]
public class InventoryLotAppService :
    CUInventoryReadOnlyAppService<InventoryLot, InventoryLotDto, InventoryLotDto, Guid, GetInventoryLotListDto>,
    IInventoryLotAppService
{
    private readonly IInventoryLotRepository _repository;

    public InventoryLotAppService(IInventoryLotRepository repository)
        : base(repository)
    {
        _repository = repository;

        GetPolicyName = CUInventoryPermissions.InventoryLots.Default;
        GetListPolicyName = CUInventoryPermissions.InventoryLots.Default;
    }

    protected override async Task<IQueryable<InventoryLot>> CreateFilteredQueryAsync(GetInventoryLotListDto input)
    {
        var query = await _repository.GetQueryableAsync();
        return query
            .WhereIf(input.WarehouseId.HasValue, l => l.WarehouseId == input.WarehouseId!.Value)
            .WhereIf(input.ProductId.HasValue, l => l.ProductId == input.ProductId!.Value)
            .WhereIf(input.SupplierId.HasValue, l => l.SupplierId == input.SupplierId!.Value)
            .WhereIf(input.HasRemaining == true, l => l.RemainingQuantity.Value > 0);
    }
}
