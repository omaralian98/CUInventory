using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class StockTransferHasNoLinesDomainException : BusinessException
{
    public StockTransferHasNoLinesDomainException(Guid stockTransferId)
        : base(CUInventoryDomainErrorCodes.StockTransferHasNoLines)
    {
        WithData("StockTransferId", stockTransferId);
    }
}
