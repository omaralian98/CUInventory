using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Catalog.Dtos;

public class CategoryDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int OrderIndex { get; set; }
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
