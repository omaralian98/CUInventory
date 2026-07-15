using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory.Repositories;

public interface IInventoryBalanceRepository : IRepository<InventoryBalance, Guid>
{
    Task<InventoryBalance?> GetByWarehouseAndProductOrDefaultAsync(Guid warehouseId, Guid productId);

    Task<InventoryBalance> InsertOrGetAsync(InventoryBalance balance);
}
