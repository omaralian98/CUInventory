using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class StockTransferCannotBeDeletedDomainException : BusinessException
{
    public StockTransferCannotBeDeletedDomainException(Guid stockTransferId, StockTransferStatus status)
        : base(CUInventoryDomainErrorCodes.StockTransferCannotBeDeleted)
    {
        WithData("StockTransferId", stockTransferId);
        WithData("Status", status);
    }
}
