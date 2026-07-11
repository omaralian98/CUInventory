using System.Collections.Generic;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory.Allocation;

public interface IInventoryAllocationStrategy
{
    AllocationStrategyKind Kind { get; }

    IReadOnlyList<AllocationResult> Allocate(AllocationRequest request, IReadOnlyList<InventoryLot> candidates);
}
