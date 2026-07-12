using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CUInventory.Inventory;

namespace CUInventory.Sales.Dtos;

public class CreateSaleDto
{
    [Required]
    [MinLength(1)]
    public List<CreateSaleLineDto> Lines { get; set; } = [];
}

public class CreateSaleLineDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(typeof(decimal), ValidationConsts.PositiveDecimalMin, ValidationConsts.DecimalMax)]
    public decimal Quantity { get; set; }

    [Range(typeof(decimal), ValidationConsts.ZeroDecimalMin, ValidationConsts.DecimalMax)]
    public decimal UnitPrice { get; set; }

    public AllocationStrategyKind Kind { get; set; } = AllocationStrategyKind.Fifo;

    public Guid? WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? LotId { get; set; }
}
