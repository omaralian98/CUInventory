using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class InventoryLotInsufficientRemainingDomainException : BusinessException
{
    public InventoryLotInsufficientRemainingDomainException(Guid inventoryLotId, decimal requested, decimal remaining)
        : base(CUInventoryDomainErrorCodes.InventoryLotInsufficientRemaining)
    {
        WithData("InventoryLotId", inventoryLotId);
        WithData("Requested", requested);
        WithData("Remaining", remaining);
    }
}
