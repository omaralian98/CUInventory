using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class StockTransferCannotBeCancelledDomainException : BusinessException
{
    public StockTransferCannotBeCancelledDomainException(Guid stockTransferId, StockTransferStatus status)
        : base(CUInventoryDomainErrorCodes.StockTransferCannotBeCancelled)
    {
        WithData("StockTransferId", stockTransferId);
        WithData("Status", status);
    }
}
