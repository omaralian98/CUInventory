using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class InventoryLotRestoreExceedsOriginalDomainException : BusinessException
{
    public InventoryLotRestoreExceedsOriginalDomainException(Guid inventoryLotId, decimal requested, decimal remaining, decimal original)
        : base(CUInventoryDomainErrorCodes.InventoryLotRestoreExceedsOriginal)
    {
        WithData("InventoryLotId", inventoryLotId);
        WithData("Requested", requested);
        WithData("Remaining", remaining);
        WithData("Original", original);
    }
}
