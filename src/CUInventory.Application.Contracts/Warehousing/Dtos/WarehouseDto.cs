using System;
using CUInventory.Shared.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Warehousing.Dtos;

public class WarehouseDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
    public bool IsActive { get; set; }
    public int OrderIndex { get; set; }
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
