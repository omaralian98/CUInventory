using System;
using CUInventory.Inventory.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Inventory;

public interface IInventoryLotAppService :
    IReadOnlyAppService<InventoryLotDto, InventoryLotDto, Guid, GetInventoryLotListDto>
{
}
