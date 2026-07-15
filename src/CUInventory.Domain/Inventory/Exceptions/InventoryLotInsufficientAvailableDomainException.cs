using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class InventoryLotInsufficientAvailableDomainException : BusinessException
{
    public InventoryLotInsufficientAvailableDomainException(Guid inventoryLotId, decimal requested, decimal available)
        : base(CUInventoryDomainErrorCodes.InventoryLotInsufficientAvailable)
    {
        WithData("InventoryLotId", inventoryLotId);
        WithData("Requested", requested);
        WithData("Available", available);
    }
}
