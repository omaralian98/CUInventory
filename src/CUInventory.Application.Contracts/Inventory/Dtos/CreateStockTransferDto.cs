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

    [Range(typeof(decimal), "0.0000000001", "79228162514264337593543950335")]
    public decimal Quantity { get; set; }
}
