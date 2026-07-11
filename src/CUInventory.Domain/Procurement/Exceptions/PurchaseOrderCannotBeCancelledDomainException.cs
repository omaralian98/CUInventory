using System;
using Volo.Abp;

namespace CUInventory.Procurement.Exceptions;

public class PurchaseOrderCannotBeCancelledDomainException : BusinessException
{
    public PurchaseOrderCannotBeCancelledDomainException(Guid purchaseOrderId, PurchaseOrderStatus status)
        : base(CUInventoryDomainErrorCodes.PurchaseOrderCannotBeCancelled)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
        WithData("Status", status);
    }
}
