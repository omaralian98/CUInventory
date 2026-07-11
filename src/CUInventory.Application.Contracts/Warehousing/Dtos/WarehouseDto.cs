using System;
using CUInventory.Shared.Dtos;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Warehousing.Dtos;

public class WarehouseDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
    public bool IsActive { get; set; }
    public int OrderIndex { get; set; }
}
