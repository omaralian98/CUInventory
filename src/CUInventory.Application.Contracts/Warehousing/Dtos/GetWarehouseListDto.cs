using Volo.Abp.Application.Dtos;

namespace CUInventory.Warehousing.Dtos;

public class GetWarehouseListDto : PagedAndSortedResultRequestDto, IHasIncludeInactive
{
    public string? Filter { get; set; }
    public bool IncludeInactive { get; set; }
}
