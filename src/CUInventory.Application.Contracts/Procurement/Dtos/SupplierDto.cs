using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Procurement.Dtos;

public class SupplierDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; } = string.Empty;
    public ContactInfoDto Contact { get; set; } = new();
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
