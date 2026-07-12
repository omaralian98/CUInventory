using System.ComponentModel.DataAnnotations;

namespace CUInventory.Inventory.Dtos;

public class SetLowStockThresholdDto
{
    [Range(typeof(decimal), ValidationConsts.ZeroDecimalMin, ValidationConsts.DecimalMax)]
    public decimal? Threshold { get; set; }
}
