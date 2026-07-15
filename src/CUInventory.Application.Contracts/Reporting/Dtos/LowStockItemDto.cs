using System;

namespace CUInventory.Reporting.Dtos;

public class LowStockItemDto
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal? LowStockThreshold { get; set; }
}
