using System;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Reporting.Dtos;

public class ReportFilterInput : PagedAndSortedResultRequestDto
{
    public Guid? WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? ProductId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
