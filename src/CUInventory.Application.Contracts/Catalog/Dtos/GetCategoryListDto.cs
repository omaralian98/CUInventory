using Volo.Abp.Application.Dtos;

namespace CUInventory.Catalog.Dtos;

public class GetCategoryListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsActive { get; set; }
}
