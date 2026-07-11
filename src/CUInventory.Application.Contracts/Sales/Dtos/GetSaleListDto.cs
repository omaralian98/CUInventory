using Volo.Abp.Application.Dtos;

namespace CUInventory.Sales.Dtos;

public class GetSaleListDto : PagedAndSortedResultRequestDto
{
    public SaleStatus? Status { get; set; }
}
