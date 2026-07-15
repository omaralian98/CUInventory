using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Warehousing.Dtos;

public class ShipmentDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public Guid PurchaseOrderId { get; set; }
    public Guid SupplierId { get; set; }
    public Guid DestinationWarehouseId { get; set; }
    public ShipmentStatus Status { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public List<ShipmentLineDto> Lines { get; set; } = [];
    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class ShipmentLineDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
