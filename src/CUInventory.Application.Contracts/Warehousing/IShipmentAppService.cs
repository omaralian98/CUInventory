using System;
using System.Threading.Tasks;
using CUInventory.Shared.Dtos;
using CUInventory.Warehousing.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Warehousing;

public interface IShipmentAppService :
    IReadOnlyAppService<ShipmentDto, ShipmentDto, Guid, GetShipmentListDto>
{
    Task<ShipmentDto> CreateAsync(CreateShipmentDto input);
    Task DeleteAsync(Guid id);
    Task<ShipmentDto> DispatchAsync(Guid id, ConcurrencyStampDto input);
    Task<ShipmentDto> ReceiveAsync(Guid id, ConcurrencyStampDto input);
}
