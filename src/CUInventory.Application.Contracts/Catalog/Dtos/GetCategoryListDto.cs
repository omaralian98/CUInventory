using Volo.Abp.Application.Dtos;

namespace CUInventory.Catalog.Dtos;

public class GetCategoryListDto : PagedAndSortedResultRequestDto, IHasIncludeInactive
{
    public string? Filter { get; set; }
    public bool IncludeInactive { get; set; }
}
