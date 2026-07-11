using System;
using CUInventory.Warehousing.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Warehousing;

public interface IWarehouseAppService :
    ICrudAppService<WarehouseDto, Guid, GetWarehouseListDto, CreateWarehouseDto, UpdateWarehouseDto>
{
}
