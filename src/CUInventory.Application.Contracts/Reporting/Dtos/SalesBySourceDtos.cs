using System;
using System.Collections.Generic;

namespace CUInventory.Reporting.Dtos;

/// <summary>
/// One row of "how much of Product X did we sell that originally came from Supplier Y", with the
/// financial picture behind it.
/// </summary>
public class SalesBySourceItemDto
{
    public Guid? SupplierId { get; set; }
    public Guid ProductId { get; set; }
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal GrossMargin { get; set; }
}

public class SalesBySourceReportDto
{
    public List<SalesBySourceItemDto> Items { get; set; } = [];

    /// <summary>Total number of grouped rows across the whole filtered result (for paging).</summary>
    public long TotalCount { get; set; }

    public decimal TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalGrossMargin { get; set; }
}

public class SalesSourceDetailDto
{
    public Guid SaleId { get; set; }
    public Guid SaleLineId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? InventoryLotId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal GrossMargin { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}
