using System;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Inventory;

public class EfCoreInventoryBalanceRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : CUInventoryEfCoreRepository<InventoryBalance, Guid>(dbContextProvider), IInventoryBalanceRepository
{
    public async Task<InventoryBalance?> GetByWarehouseAndProductOrDefaultAsync(Guid warehouseId, Guid productId)
    {
        var query = await WithDetailsAsync();
        return await query.FirstOrDefaultAsync(
            x => x.WarehouseId == warehouseId && x.ProductId == productId, GetCancellationToken());
    }

    public async Task<InventoryBalance> InsertOrGetAsync(InventoryBalance balance)
    {
        try
        {
            return await InsertAsync(balance, autoSave: true);
        }
        catch (DbUpdateException)
        {
            var dbContext = await GetDbContextAsync();
            dbContext.Entry(balance).State = EntityState.Detached;

            var existing = await GetByWarehouseAndProductOrDefaultAsync(balance.WarehouseId, balance.ProductId);
            if (existing is null)
            {
                throw;
            }

            return existing;
        }
    }
}
