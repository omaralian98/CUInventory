using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Procurement.Dtos;

public class SupplierDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public ContactInfoDto Contact { get; set; } = new();
}
