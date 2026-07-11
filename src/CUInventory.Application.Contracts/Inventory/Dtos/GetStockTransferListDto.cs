using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Inventory.Dtos;

public class GetStockTransferListDto : PagedAndSortedResultRequestDto
{
    public Guid? SourceWarehouseId { get; set; }
    public Guid? DestinationWarehouseId { get; set; }
    public StockTransferStatus? Status { get; set; }
}
