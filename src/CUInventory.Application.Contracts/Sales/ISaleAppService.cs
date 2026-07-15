using System;
using System.Threading.Tasks;
using CUInventory.Sales.Dtos;
using CUInventory.Shared.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Sales;

public interface ISaleAppService :
    IReadOnlyAppService<SaleDto, SaleDto, Guid, GetSaleListDto>
{
    Task<SaleDto> CreateAsync(CreateSaleDto input);
    Task DeleteAsync(Guid id);
    Task<SaleDto> ConfirmAsync(Guid id, ConcurrencyStampDto input);
    Task<SaleDto> CancelAsync(Guid id, ConcurrencyStampDto input);
}
