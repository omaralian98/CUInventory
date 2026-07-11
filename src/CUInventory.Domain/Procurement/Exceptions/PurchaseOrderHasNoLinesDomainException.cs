using System;
using Volo.Abp;

namespace CUInventory.Procurement.Exceptions;

public class PurchaseOrderHasNoLinesDomainException : BusinessException
{
    public PurchaseOrderHasNoLinesDomainException(Guid purchaseOrderId)
        : base(CUInventoryDomainErrorCodes.PurchaseOrderHasNoLines)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
    }
}
