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

    [Range(typeof(decimal), ValidationConsts.PositiveDecimalMin, ValidationConsts.DecimalMax)]
    public decimal OrderedQuantity { get; set; }

    [Range(typeof(decimal), ValidationConsts.ZeroDecimalMin, ValidationConsts.DecimalMax)]
    public decimal UnitCost { get; set; }
}
