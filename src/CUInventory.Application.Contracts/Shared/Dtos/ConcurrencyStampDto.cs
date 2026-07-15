using Volo.Abp.Domain.Entities;

namespace CUInventory.Shared.Dtos;

public class ConcurrencyStampDto : IHasConcurrencyStamp
{
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
