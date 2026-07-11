using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class InsufficientReservedStockDomainException : BusinessException
{
    public InsufficientReservedStockDomainException(Guid inventoryBalanceId, decimal requested, decimal reserved)
        : base(CUInventoryDomainErrorCodes.InsufficientReservedStock)
    {
        WithData("InventoryBalanceId", inventoryBalanceId);
        WithData("Requested", requested);
        WithData("Reserved", reserved);
    }
}
