using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Inventory.Dtos;

public class InventoryLotDto : FullAuditedEntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? ShipmentLineId { get; set; }
    public InventoryLotSource Source { get; set; }
    public decimal OriginalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime ReceivedAt { get; set; }
}
