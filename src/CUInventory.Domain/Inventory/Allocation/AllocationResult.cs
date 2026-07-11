using System;
using CUInventory.ValueObjects;

namespace CUInventory.Inventory.Allocation;

public record AllocationResult(Guid LotId, Guid WarehouseId, Guid? SupplierId, Money UnitCost, decimal Quantity);
