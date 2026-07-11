using System;
using CUInventory.Inventory;

namespace CUInventory.Sales;

public record SaleLineRequest(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    AllocationStrategyKind Kind = AllocationStrategyKind.Fifo,
    Guid? WarehouseId = null,
    Guid? SupplierId = null,
    Guid? LotId = null);
