using System.Collections.Generic;
using System.Linq;
using CUInventory.Inventory.Aggregates;
using Volo.Abp.DependencyInjection;

namespace CUInventory.Inventory.Allocation;

[ExposeServices(typeof(IInventoryAllocationStrategy))]
public class FifoAllocationStrategy : InventoryAllocationStrategyBase, ITransientDependency
{
    public override AllocationStrategyKind Kind => AllocationStrategyKind.Fifo;

    protected override IEnumerable<InventoryLot> SelectCandidates(AllocationRequest request, IReadOnlyList<InventoryLot> candidates)
        => candidates.OrderBy(lot => lot.ReceivedAt).ThenBy(lot => lot.Id);
}
