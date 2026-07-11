using System.Collections.Generic;
using System.Linq;
using CUInventory.Common;
using CUInventory.Inventory.Aggregates;
using Volo.Abp.DependencyInjection;

namespace CUInventory.Inventory.Allocation;

[ExposeServices(typeof(IInventoryAllocationStrategy))]
public class SpecificWarehouseAllocationStrategy : InventoryAllocationStrategyBase, ITransientDependency
{
    public override AllocationStrategyKind Kind => AllocationStrategyKind.SpecificWarehouse;

    protected override IEnumerable<InventoryLot> SelectCandidates(AllocationRequest request, IReadOnlyList<InventoryLot> candidates)
    {
        Guard.NotEmpty(request.WarehouseIds, nameof(request.WarehouseIds));

        return candidates
            .Where(lot => request.WarehouseIds.Contains(lot.WarehouseId))
            .OrderBy(lot => lot.ReceivedAt)
            .ThenBy(lot => lot.Id);
    }
}
