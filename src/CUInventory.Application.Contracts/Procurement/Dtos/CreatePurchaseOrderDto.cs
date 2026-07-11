using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CUInventory.Procurement.Dtos;

public class CreatePurchaseOrderDto
{
    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    public Guid DestinationWarehouseId { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreatePurchaseOrderLineDto> Lines { get; set; } = [];
}

public class CreatePurchaseOrderLineDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(typeof(decimal), "0.0000000001", "79228162514264337593543950335")]
    public decimal OrderedQuantity { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal UnitCost { get; set; }
}
