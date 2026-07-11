using System;
using Volo.Abp;

namespace CUInventory.Procurement.Exceptions;

public class PurchaseOrderLineNotFoundDomainException : BusinessException
{
    public PurchaseOrderLineNotFoundDomainException(Guid purchaseOrderId, Guid productId)
        : base(CUInventoryDomainErrorCodes.PurchaseOrderLineNotFound)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
        WithData("ProductId", productId);
    }
}
