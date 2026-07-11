using System.ComponentModel.DataAnnotations;
using CUInventory.Shared.Dtos;

namespace CUInventory.Warehousing.Dtos;

public class CreateWarehouseDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public AddressDto Address { get; set; } = new();

    public bool IsActive { get; set; } = true;

    public int OrderIndex { get; set; }
}
