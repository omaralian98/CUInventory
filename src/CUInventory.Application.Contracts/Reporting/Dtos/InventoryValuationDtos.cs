using System;
using System.Collections.Generic;

namespace CUInventory.Reporting.Dtos;

public class InventoryValuationItemDto
{
    public Guid WarehouseId { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal TotalValue { get; set; }
}

public class InventoryValuationReportDto
{
    public List<InventoryValuationItemDto> Items { get; set; } = [];

    /// <summary>Total number of grouped rows across the whole filtered result (for paging).</summary>
    public long TotalCount { get; set; }

    public decimal GrandTotalQuantity { get; set; }
    public decimal GrandTotalValue { get; set; }
}
