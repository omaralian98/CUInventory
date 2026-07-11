using System;
using System.Collections.Generic;
using System.Linq;
using CUInventory.Inventory.Aggregates;
using Volo.Abp.Domain.Services;

namespace CUInventory.Inventory.Allocation;

public class InventoryAllocationService(IEnumerable<IInventoryAllocationStrategy> strategies) : DomainService
{
    public IReadOnlyList<AllocationResult> Allocate(AllocationRequest request, IReadOnlyList<InventoryLot> candidates)
    {
        var strategy = strategies.FirstOrDefault(s => s.Kind == request.Kind)
                       ?? throw new ArgumentException($"No allocation strategy is registered for '{request.Kind}'.", nameof(request));

        return strategy.Allocate(request, candidates);
    }
}
