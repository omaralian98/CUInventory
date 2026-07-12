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

        // Since the caller is Expecting to get the balance meaning he shouldn't consider himself with the responsibility of creating the balance, we will create it with and save it.
        var newBalance = new InventoryBalance(GuidGenerator.Create(), warehouseId, productId, lowStockThreshold);
        
        // We use autosave so if there was another call to GetOrCreateAsync for the same warehouse and product, it will fetch the autosaved one instead of creating another one.
        await inventoryBalanceRepository.InsertAsync(newBalance, autoSave: true);
        return newBalance;
    }
}
