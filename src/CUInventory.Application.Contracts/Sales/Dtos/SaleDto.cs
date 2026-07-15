using System;
using System.Collections.Generic;
using CUInventory.Inventory;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Sales.Dtos;

public class SaleDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public SaleStatus Status { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public List<SaleLineDto> Lines { get; set; } = [];
    public string ConcurrencyStamp { get; set; } = string.Empty;
}

public class SaleLineDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public AllocationStrategyKind Kind { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? LotId { get; set; }
    public List<SaleAllocationDto> Allocations { get; set; } = [];
}

public class SaleAllocationDto
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? InventoryLotId { get; set; }
    public Guid? SupplierId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitCost { get; set; }
    public bool IsReserved { get; set; }
}
