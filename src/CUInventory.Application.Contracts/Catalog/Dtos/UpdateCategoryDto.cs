using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Catalog.Dtos;

public class UpdateCategoryDto : IHasConcurrencyStamp
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int OrderIndex { get; set; }

    public string ConcurrencyStamp { get; set; } = string.Empty;
}
