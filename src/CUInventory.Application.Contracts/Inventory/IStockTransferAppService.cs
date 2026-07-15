using System;
using System.Threading.Tasks;
using CUInventory.Inventory.Dtos;
using CUInventory.Shared.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Inventory;

public interface IStockTransferAppService :
    IReadOnlyAppService<StockTransferDto, StockTransferDto, Guid, GetStockTransferListDto>
{
    Task<StockTransferDto> CreateAsync(CreateStockTransferDto input);
    Task DeleteAsync(Guid id);
    Task<StockTransferDto> DispatchAsync(Guid id, ConcurrencyStampDto input);
    Task<StockTransferDto> ReceiveAsync(Guid id, ConcurrencyStampDto input);
    Task<StockTransferDto> CancelAsync(Guid id, ConcurrencyStampDto input);
}
