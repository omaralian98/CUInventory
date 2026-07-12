using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Catalog.Dtos;

public class GetProductListDto : PagedAndSortedResultRequestDto, IHasIncludeInactive
{
    public string? Filter { get; set; }
    public bool IncludeInactive { get; set; }
    public Guid? CategoryId { get; set; }
}
