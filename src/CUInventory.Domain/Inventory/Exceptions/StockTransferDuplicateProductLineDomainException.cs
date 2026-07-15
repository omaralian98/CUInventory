using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class StockTransferDuplicateProductLineDomainException : BusinessException
{
    public StockTransferDuplicateProductLineDomainException(Guid stockTransferId, Guid productId)
        : base(CUInventoryDomainErrorCodes.StockTransferDuplicateProductLine)
    {
        WithData("StockTransferId", stockTransferId);
        WithData("ProductId", productId);
    }
}
