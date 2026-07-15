using System.ComponentModel.DataAnnotations;
using CUInventory.Shared.Dtos;

namespace CUInventory.Procurement.Dtos;

public class ContactInfoDto
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(32)]
    [RegularExpression(@"^\+?(?:[\s\-()]*\d){7,15}[\s\-()]*$", ErrorMessage = "The phone number format is invalid.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    public AddressDto Address { get; set; } = new();
}
