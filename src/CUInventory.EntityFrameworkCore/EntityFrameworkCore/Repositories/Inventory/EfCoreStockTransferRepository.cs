using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Inventory;

public class EfCoreStockTransferRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : EfCoreRepository<CUInventoryDbContext, StockTransfer, Guid>(dbContextProvider), IStockTransferRepository
{
    public override async Task<IQueryable<StockTransfer>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(x => x.Lines)
            .Include(x => x.Allocations);
    }
}
