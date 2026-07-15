using System;

namespace CUInventory.Reporting;

public record ReportQueryFilter
{
    public Guid? WarehouseId { get; init; }
    public Guid? SupplierId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? ProductId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    public int SkipCount { get; init; }
    public int MaxResultCount { get; init; } = 10;
    public string? Sorting { get; init; }
}
