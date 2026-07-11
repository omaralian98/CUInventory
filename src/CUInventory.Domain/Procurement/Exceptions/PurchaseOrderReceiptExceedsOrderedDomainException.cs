using System;
using Volo.Abp;

namespace CUInventory.Procurement.Exceptions;

public class PurchaseOrderReceiptExceedsOrderedDomainException : BusinessException
{
    public PurchaseOrderReceiptExceedsOrderedDomainException(Guid purchaseOrderId, Guid productId, decimal requested, decimal outstanding)
        : base(CUInventoryDomainErrorCodes.PurchaseOrderReceiptExceedsOrdered)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
        WithData("ProductId", productId);
        WithData("Requested", requested);
        WithData("Outstanding", outstanding);
    }
}
