using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class StockTransferSameWarehouseDomainException : BusinessException
{
    public StockTransferSameWarehouseDomainException(Guid warehouseId)
        : base(CUInventoryDomainErrorCodes.StockTransferSameWarehouse)
    {
        WithData("WarehouseId", warehouseId);
    }
}
