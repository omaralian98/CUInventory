using System;
using Volo.Abp;

namespace CUInventory.Warehousing.Exceptions;

public class ShipmentExceedsOutstandingDomainException : BusinessException
{
    public ShipmentExceedsOutstandingDomainException(Guid purchaseOrderId, Guid productId, decimal requested, decimal outstanding)
        : base(CUInventoryDomainErrorCodes.ShipmentExceedsOutstanding)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
        WithData("ProductId", productId);
        WithData("Requested", requested);
        WithData("Outstanding", outstanding);
    }
}
