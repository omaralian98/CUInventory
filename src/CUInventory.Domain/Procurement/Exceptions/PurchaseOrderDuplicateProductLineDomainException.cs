using System;
using Volo.Abp;

namespace CUInventory.Procurement.Exceptions;

public class PurchaseOrderDuplicateProductLineDomainException : BusinessException
{
    public PurchaseOrderDuplicateProductLineDomainException(Guid purchaseOrderId, Guid productId)
        : base(CUInventoryDomainErrorCodes.PurchaseOrderDuplicateProductLine)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
        WithData("ProductId", productId);
    }
}
