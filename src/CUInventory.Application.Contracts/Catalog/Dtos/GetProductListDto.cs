using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Catalog.Dtos;

public class GetProductListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
    public Guid? CategoryId { get; set; }
}
