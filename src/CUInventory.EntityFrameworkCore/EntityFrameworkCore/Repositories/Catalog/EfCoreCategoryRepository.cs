using System;
using System.Threading.Tasks;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Catalog;

public class EfCoreCategoryRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : CUInventoryEfCoreRepository<Category, Guid>(dbContextProvider), ICategoryRepository
{
    public async Task<bool> ExistsAsync(string name)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(x => x.Name == name, GetCancellationToken());
    }
}
