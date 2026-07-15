using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Procurement.Dtos;

public class GetPurchaseOrderListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public PurchaseOrderStatus? Status { get; set; }
    public List<PurchaseOrderStatus>? Statuses { get; set; }
}
