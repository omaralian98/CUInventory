using System;
using Volo.Abp;

namespace CUInventory.Inventory.Exceptions;

public class InsufficientStockDomainException : BusinessException
{
    public InsufficientStockDomainException(Guid productId, decimal requested, decimal available)
        : base(CUInventoryDomainErrorCodes.InsufficientStock)
    {
        WithData("ProductId", productId);
        WithData("Requested", requested);
        WithData("Available", available);
    }

    public InsufficientStockDomainException(Guid productId, Guid warehouseId, decimal requested, decimal available)
        : this(productId, requested, available)
    {
        WithData("WarehouseId", warehouseId);
    }
}
