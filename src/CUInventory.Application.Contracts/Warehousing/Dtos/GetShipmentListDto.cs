using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Warehousing.Dtos;

public class GetShipmentListDto : PagedAndSortedResultRequestDto
{
    public Guid? PurchaseOrderId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public ShipmentStatus? Status { get; set; }
}
