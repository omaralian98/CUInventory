using System.Collections.Generic;
using System.Linq;
using CUInventory.Common;
using CUInventory.Inventory.Aggregates;
using Volo.Abp.DependencyInjection;

namespace CUInventory.Inventory.Allocation;

[ExposeServices(typeof(IInventoryAllocationStrategy))]
public class SpecificLotAllocationStrategy : InventoryAllocationStrategyBase, ITransientDependency
{
    public override AllocationStrategyKind Kind => AllocationStrategyKind.SpecificLot;

    protected override IEnumerable<InventoryLot> SelectCandidates(AllocationRequest request, IReadOnlyList<InventoryLot> candidates)
    {
        Guard.NotEmpty(request.LotIds, nameof(request.LotIds));

        return candidates
            .Where(lot => request.LotIds.Contains(lot.Id))
            .OrderBy(lot => lot.ReceivedAt)
            .ThenBy(lot => lot.Id);
    }
}
