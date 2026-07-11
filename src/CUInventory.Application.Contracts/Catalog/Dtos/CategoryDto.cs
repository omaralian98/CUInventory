using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Catalog.Dtos;

public class CategoryDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int OrderIndex { get; set; }
}
