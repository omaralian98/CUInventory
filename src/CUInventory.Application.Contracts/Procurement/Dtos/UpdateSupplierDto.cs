using System.ComponentModel.DataAnnotations;

namespace CUInventory.Procurement.Dtos;

public class UpdateSupplierDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public ContactInfoDto Contact { get; set; } = new();
}
