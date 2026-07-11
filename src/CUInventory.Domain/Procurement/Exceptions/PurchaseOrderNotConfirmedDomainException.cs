using System;
using Volo.Abp;

namespace CUInventory.Procurement.Exceptions;

public class PurchaseOrderNotConfirmedDomainException : BusinessException
{
    public PurchaseOrderNotConfirmedDomainException(Guid purchaseOrderId, PurchaseOrderStatus status)
        : base(CUInventoryDomainErrorCodes.PurchaseOrderNotConfirmed)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
        WithData("Status", status);
    }
}
