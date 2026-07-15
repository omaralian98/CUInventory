using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories;

public class CUInventoryEfCoreRepository<TEntity, TKey>(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : EfCoreRepository<CUInventoryDbContext, TEntity, TKey>(dbContextProvider)
    where TEntity : class, IEntity<TKey>
{
}
