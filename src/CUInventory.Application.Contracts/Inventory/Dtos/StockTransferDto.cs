using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Inventory.Dtos;

public class StockTransferDto : FullAuditedEntityDto<Guid>
{
    public Guid SourceWarehouseId { get; set; }
    public Guid DestinationWarehouseId { get; set; }
    public StockTransferStatus Status { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public List<StockTransferLineDto> Lines { get; set; } = [];
    public List<TransferAllocationDto> Allocations { get; set; } = [];
}

public class StockTransferLineDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
}

public class TransferAllocationDto
{
    public Guid Id { get; set; }
    public Guid SourceLotId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal UnitCost { get; set; }
    public decimal Quantity { get; set; }
}
