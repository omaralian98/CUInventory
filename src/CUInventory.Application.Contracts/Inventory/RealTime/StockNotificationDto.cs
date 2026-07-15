using System;

namespace CUInventory.Inventory.RealTime;

public class StockNotificationDto
{
    public StockNotificationType Type { get; set; }
    public Guid InventoryBalanceId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal? LowStockThreshold { get; set; }
    public bool IsBelowThreshold { get; set; }
    public DateTime OccurredAt { get; set; }
    public Guid? TenantId { get; set; }
}
