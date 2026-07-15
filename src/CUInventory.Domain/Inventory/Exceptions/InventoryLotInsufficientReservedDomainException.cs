using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class InventoryLotInsufficientReservedDomainException : BusinessException
{
    public InventoryLotInsufficientReservedDomainException(Guid inventoryLotId, decimal requested, decimal reserved)
        : base(CUInventoryDomainErrorCodes.InventoryLotInsufficientReserved)
    {
        WithData("InventoryLotId", inventoryLotId);
        WithData("Requested", requested);
        WithData("Reserved", reserved);
    }
}
