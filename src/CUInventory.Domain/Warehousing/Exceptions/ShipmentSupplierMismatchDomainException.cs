using System;
using Volo.Abp;

namespace CUInventory.Warehousing.Exceptions;

public class ShipmentSupplierMismatchDomainException : BusinessException
{
    public ShipmentSupplierMismatchDomainException(Guid purchaseOrderId, Guid supplierId, Guid expectedSupplierId)
        : base(CUInventoryDomainErrorCodes.ShipmentSupplierMismatch)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
        WithData("SupplierId", supplierId);
        WithData("ExpectedSupplierId", expectedSupplierId);
    }
}
