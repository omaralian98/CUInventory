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

    [Range(typeof(decimal), "0.0000000001", "79228162514264337593543950335")]
    public decimal Quantity { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal UnitPrice { get; set; }

    public AllocationStrategyKind Kind { get; set; } = AllocationStrategyKind.Fifo;

    public Guid? WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? LotId { get; set; }
}
