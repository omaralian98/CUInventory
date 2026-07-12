using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CUInventory.Warehousing.Dtos;

public class CreateShipmentDto
{
    [Required]
    public Guid PurchaseOrderId { get; set; }

    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    public Guid DestinationWarehouseId { get; set; }

    [Required]
    [MinLength(1)]
    public List<CreateShipmentLineDto> Lines { get; set; } = [];
}

public class CreateShipmentLineDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(typeof(decimal), ValidationConsts.PositiveDecimalMin, ValidationConsts.DecimalMax)]
    public decimal Quantity { get; set; }

    [Range(typeof(decimal), ValidationConsts.ZeroDecimalMin, ValidationConsts.DecimalMax)]
    public decimal UnitCost { get; set; }
}
