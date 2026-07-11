using System;
using CUInventory.Procurement.Dtos;
using Volo.Abp.Application.Services;

namespace CUInventory.Procurement;

public interface ISupplierAppService :
    ICrudAppService<SupplierDto, Guid, GetSupplierListDto, CreateSupplierDto, UpdateSupplierDto>
{
}
