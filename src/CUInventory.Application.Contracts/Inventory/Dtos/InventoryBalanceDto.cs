using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Inventory.Dtos;

public class InventoryBalanceDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal? LowStockThreshold { get; set; }
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
