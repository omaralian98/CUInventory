using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class StockTransferNotDispatchedDomainException : BusinessException
{
    public StockTransferNotDispatchedDomainException(Guid stockTransferId, StockTransferStatus status)
        : base(CUInventoryDomainErrorCodes.StockTransferNotDispatched)
    {
        WithData("StockTransferId", stockTransferId);
        WithData("Status", status);
    }
}
