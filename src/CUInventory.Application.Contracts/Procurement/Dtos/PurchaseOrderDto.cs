using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Procurement.Dtos;

public class PurchaseOrderDto : FullAuditedEntityDto<Guid>
{
    public Guid SupplierId { get; set; }
    public Guid DestinationWarehouseId { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public List<PurchaseOrderLineDto> Lines { get; set; } = [];
}

public class PurchaseOrderLineDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal OutstandingQuantity { get; set; }
    public bool IsFullyReceived { get; set; }
}
