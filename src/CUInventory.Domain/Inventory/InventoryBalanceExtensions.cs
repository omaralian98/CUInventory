using System;
using System.Collections.Generic;
using System.Linq;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Exceptions;

namespace CUInventory.Inventory;

public static class InventoryBalanceExtensions
{
    public static InventoryBalance FindRequired(this List<InventoryBalance> balances, Guid warehouseId, Guid productId)
        => balances.FirstOrDefault(b => b.WarehouseId == warehouseId && b.ProductId == productId)
           ?? throw new MissingInventoryBalanceDomainException(warehouseId, productId);
}
