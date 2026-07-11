using System.Collections.Generic;
using System.Linq;
using CUInventory.Common;
using CUInventory.Inventory.Aggregates;
using Volo.Abp.DependencyInjection;

namespace CUInventory.Inventory.Allocation;

[ExposeServices(typeof(IInventoryAllocationStrategy))]
public class SpecificSupplierAllocationStrategy : InventoryAllocationStrategyBase, ITransientDependency
{
    public override AllocationStrategyKind Kind => AllocationStrategyKind.SpecificSupplier;

    protected override IEnumerable<InventoryLot> SelectCandidates(AllocationRequest request, IReadOnlyList<InventoryLot> candidates)
    {
        Guard.NotNull(request.SupplierId, nameof(request.SupplierId));

        return candidates
            .Where(lot => lot.SupplierId == request.SupplierId)
            .OrderBy(lot => lot.ReceivedAt)
            .ThenBy(lot => lot.Id);
    }
}
