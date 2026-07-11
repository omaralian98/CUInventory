using System;
using Volo.Abp;

namespace CUInventory.Procurement.Exceptions;

public class PurchaseOrderNotInDraftStateDomainException : BusinessException
{
    public PurchaseOrderNotInDraftStateDomainException(Guid purchaseOrderId, PurchaseOrderStatus status)
        : base(CUInventoryDomainErrorCodes.PurchaseOrderNotInDraftState)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
        WithData("Status", status);
    }
}
