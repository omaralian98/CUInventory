using System.ComponentModel.DataAnnotations;

namespace CUInventory.Catalog.Dtos;

public class CreateCategoryDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int OrderIndex { get; set; }
}
