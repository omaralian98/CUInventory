using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Procurement.Dtos;

public class UpdateSupplierDto : IHasConcurrencyStamp
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public ContactInfoDto Contact { get; set; } = new();

    public string ConcurrencyStamp { get; set; } = string.Empty;
}
