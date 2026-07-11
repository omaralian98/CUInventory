using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Inventory.Dtos;

public class GetInventoryBalanceListDto : PagedAndSortedResultRequestDto
{
    public Guid? WarehouseId { get; set; }
    public Guid? ProductId { get; set; }
    public bool? LowStockOnly { get; set; }
}
