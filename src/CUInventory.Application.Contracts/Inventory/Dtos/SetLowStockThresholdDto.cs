using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Inventory.Dtos;

public class SetLowStockThresholdDto : IHasConcurrencyStamp
{
    [Range(typeof(decimal), ValidationConsts.ZeroDecimalMin, ValidationConsts.DecimalMax)]
    public decimal? Threshold { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;
}
