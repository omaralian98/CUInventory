using System;
using System.Linq.Expressions;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory;

public static class LowStockRule
{
    public static Expression<Func<InventoryBalance, bool>> BelowThreshold =>
        balance => balance.LowStockThreshold != null &&
                   (balance.QuantityOnHand.Value - balance.QuantityReserved.Value) < balance.LowStockThreshold;

    public static bool IsBelowThreshold(decimal? lowStockThreshold, decimal quantityAvailable)
    {
        return lowStockThreshold is { } threshold && quantityAvailable < threshold;
    }
}
