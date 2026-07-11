using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Inventory.Dtos;

public class GetInventoryLotListDto : PagedAndSortedResultRequestDto
{
    public Guid? WarehouseId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public bool? HasRemaining { get; set; }
}
