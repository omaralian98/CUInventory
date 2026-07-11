using System.Collections.Generic;
using System.Linq;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Exceptions;

namespace CUInventory.Inventory.Allocation;

public abstract class InventoryAllocationStrategyBase : IInventoryAllocationStrategy
{
    public abstract AllocationStrategyKind Kind { get; }

    protected abstract IEnumerable<InventoryLot> SelectCandidates(AllocationRequest request, IReadOnlyList<InventoryLot> candidates);

    public IReadOnlyList<AllocationResult> Allocate(AllocationRequest request, IReadOnlyList<InventoryLot> candidates)
    {
        var results = new List<AllocationResult>();
        var remaining = request.Quantity;

        var eligible = SelectCandidates(request, candidates)
            .Where(lot => lot.ProductId == request.ProductId && lot.RemainingQuantity.Value > 0);

        foreach (var lot in eligible)
        {
            if (remaining <= 0)
            {
                break;
            }

            var take = remaining < lot.RemainingQuantity.Value ? remaining : lot.RemainingQuantity.Value;
            results.Add(new AllocationResult(lot.Id, lot.WarehouseId, lot.SupplierId, lot.UnitCost, take));
            remaining -= take;
        }

        if (remaining > 0)
        {
            throw new InsufficientStockDomainException(request.ProductId, request.Quantity, request.Quantity - remaining);
        }

        return results;
    }
}
