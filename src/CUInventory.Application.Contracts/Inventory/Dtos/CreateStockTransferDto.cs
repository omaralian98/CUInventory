using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CUInventory.Inventory.Dtos;

public class CreateStockTransferDto
{
    [Required]
    public Guid SourceWarehouseId { get; set; }

    [Required]
    public Guid DestinationWarehouseId { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateStockTransferLineDto> Lines { get; set; } = [];
}

public class CreateStockTransferLineDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(typeof(decimal), ValidationConsts.PositiveDecimalMin, ValidationConsts.DecimalMax)]
    public decimal Quantity { get; set; }
}
