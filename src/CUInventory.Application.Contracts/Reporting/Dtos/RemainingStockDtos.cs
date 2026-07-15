using System;
using System.Collections.Generic;
using CUInventory.Inventory;

namespace CUInventory.Reporting.Dtos;

/// <summary>
/// Remaining on-hand stock grouped by warehouse/supplier/product. Combined with a received-date range
/// this answers "what is our remaining stock from the shipment we received last March".
/// </summary>
public class RemainingStockItemDto
{
    public Guid WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid ProductId { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal ValueAtCost { get; set; }
}

public class RemainingStockReportDto
{
    public List<RemainingStockItemDto> Items { get; set; } = [];

    /// <summary>Total number of grouped rows across the whole filtered result (for paging).</summary>
    public long TotalCount { get; set; }

    public decimal TotalRemainingQuantity { get; set; }
    public decimal TotalValueAtCost { get; set; }
}

public class RemainingStockDetailDto
{
    public Guid LotId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? ShipmentLineId { get; set; }
    public InventoryLotSource Source { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal ValueAtCost { get; set; }
    public DateTime ReceivedAt { get; set; }
}
