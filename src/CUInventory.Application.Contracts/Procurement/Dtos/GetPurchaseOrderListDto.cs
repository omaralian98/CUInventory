using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Procurement.Dtos;

public class GetPurchaseOrderListDto : PagedAndSortedResultRequestDto
{
    public Guid? SupplierId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public PurchaseOrderStatus? Status { get; set; }
}
