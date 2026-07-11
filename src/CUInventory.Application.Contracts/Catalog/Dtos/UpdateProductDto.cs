using System;
using System.ComponentModel.DataAnnotations;

namespace CUInventory.Catalog.Dtos;

public class UpdateProductDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2048)]
    public string? Description { get; set; }

    [StringLength(256)]
    public string? Sku { get; set; }

    public bool IsService { get; set; }

    public Guid? CategoryId { get; set; }

    public bool IsActive { get; set; }

    public int OrderIndex { get; set; }
}
