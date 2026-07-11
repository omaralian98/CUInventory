using System;
using System.Threading.Tasks;
using CUInventory.Inventory.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Inventory;

public interface IInventoryBalanceAppService :
    IReadOnlyAppService<InventoryBalanceDto, InventoryBalanceDto, Guid, GetInventoryBalanceListDto>
{
    Task<InventoryBalanceDto> SetLowStockThresholdAsync(Guid id, SetLowStockThresholdDto input);
}
