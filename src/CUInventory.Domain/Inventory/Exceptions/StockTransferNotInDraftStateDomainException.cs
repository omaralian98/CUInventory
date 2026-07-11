using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class StockTransferNotInDraftStateDomainException : BusinessException
{
    public StockTransferNotInDraftStateDomainException(Guid stockTransferId, StockTransferStatus status)
        : base(CUInventoryDomainErrorCodes.StockTransferNotInDraftState)
    {
        WithData("StockTransferId", stockTransferId);
        WithData("Status", status);
    }
}
