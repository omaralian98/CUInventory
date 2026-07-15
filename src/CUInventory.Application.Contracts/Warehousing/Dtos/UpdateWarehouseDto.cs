using System.ComponentModel.DataAnnotations;
using CUInventory.Shared.Dtos;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Warehousing.Dtos;

public class UpdateWarehouseDto : IHasConcurrencyStamp
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public AddressDto Address { get; set; } = new();

    public bool IsActive { get; set; }

    public int OrderIndex { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;
}
