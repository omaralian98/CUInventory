using System;
using System.Threading.Tasks;
using CUInventory.Procurement.Dtos;
using CUInventory.Shared.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Procurement;

public interface IPurchaseOrderAppService :
    IReadOnlyAppService<PurchaseOrderDto, PurchaseOrderDto, Guid, GetPurchaseOrderListDto>
{
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input);
    Task DeleteAsync(Guid id);
    Task<PurchaseOrderDto> ConfirmAsync(Guid id, ConcurrencyStampDto input);
    Task<PurchaseOrderDto> CancelAsync(Guid id, ConcurrencyStampDto input);
}
