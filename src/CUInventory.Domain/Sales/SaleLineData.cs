using System;
using CUInventory.Inventory;
using CUInventory.ValueObjects;

namespace CUInventory.Sales;

public record SaleLineData(
    Guid Id,
    Guid ProductId,
    Quantity Quantity,
    Money UnitPrice,
    AllocationStrategyKind Kind,
    Guid? WarehouseId,
    Guid? SupplierId,
    Guid? LotId);
