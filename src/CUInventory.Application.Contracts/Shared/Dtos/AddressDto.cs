using System.ComponentModel.DataAnnotations;

namespace CUInventory.Shared.Dtos;

public class AddressDto
{
    [Required]
    [StringLength(128)]
    public string Governorate { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string City { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string Street { get; set; } = string.Empty;
}
