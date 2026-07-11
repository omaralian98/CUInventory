using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class MissingInventoryBalanceDomainException : BusinessException
{
    public MissingInventoryBalanceDomainException(Guid warehouseId, Guid productId)
        : base(CUInventoryDomainErrorCodes.MissingInventoryBalance)
    {
        WithData("WarehouseId", warehouseId);
        WithData("ProductId", productId);
    }
}
