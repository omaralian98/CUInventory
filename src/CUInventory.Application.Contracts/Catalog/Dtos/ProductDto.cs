using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Catalog.Dtos;

public class ProductDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public bool IsService { get; set; }
    public Guid? CategoryId { get; set; }
    public bool IsActive { get; set; }
    public int OrderIndex { get; set; }
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
