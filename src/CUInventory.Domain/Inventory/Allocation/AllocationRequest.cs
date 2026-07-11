using System;
using System.Collections.Generic;

namespace CUInventory.Inventory.Allocation;

public record AllocationRequest(
    Guid ProductId,
    decimal Quantity,
    AllocationStrategyKind Kind,
    IReadOnlyCollection<Guid>? WarehouseIds = null,
    Guid? SupplierId = null,
    IReadOnlyCollection<Guid>? LotIds = null);
