using System;
using System.Threading.Tasks;
using CUInventory.Sales.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Sales;

public interface ISaleAppService :
    IReadOnlyAppService<SaleDto, SaleDto, Guid, GetSaleListDto>
{
    Task<SaleDto> CreateAsync(CreateSaleDto input);
    Task DeleteAsync(Guid id);
    Task<SaleDto> ConfirmAsync(Guid id);
    Task<SaleDto> CancelAsync(Guid id);
}
