using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Interfaces;
using CUInventory.Inventory.Repositories;

namespace CUInventory.Inventory.Managers;

public class InventoryBalanceManager(IInventoryBalanceRepository inventoryBalanceRepository)
    : DomainService, IInventoryBalanceManager
{
    public async Task<InventoryBalance> GetOrCreateAsync(Guid warehouseId, Guid productId, decimal? lowStockThreshold = null)
    {
        var existing = await inventoryBalanceRepository.GetByWarehouseAndProductOrDefaultAsync(warehouseId, productId);
        if (existing is not null)
        {
            return existing;
        }

        var newBalance = new InventoryBalance(GuidGenerator.Create(), warehouseId, productId, lowStockThreshold);
        return await inventoryBalanceRepository.InsertOrGetAsync(newBalance);
    }
}
