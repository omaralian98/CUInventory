using Volo.Abp.Application.Dtos;

namespace CUInventory.Procurement.Dtos;

public class GetSupplierListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
