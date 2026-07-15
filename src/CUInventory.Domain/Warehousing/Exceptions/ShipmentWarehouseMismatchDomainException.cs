using System;
using Volo.Abp;

namespace CUInventory.Warehousing.Exceptions;

public class ShipmentWarehouseMismatchDomainException : BusinessException
{
    public ShipmentWarehouseMismatchDomainException(Guid purchaseOrderId, Guid warehouseId, Guid expectedWarehouseId)
        : base(CUInventoryDomainErrorCodes.ShipmentWarehouseMismatch)
    {
        WithData("PurchaseOrderId", purchaseOrderId);
        WithData("WarehouseId", warehouseId);
        WithData("ExpectedWarehouseId", expectedWarehouseId);
    }
}
