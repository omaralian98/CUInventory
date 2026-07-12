using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CUInventory.Abstractions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories;

public class CUInventoryEfCoreRepository<TEntity, TKey>(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : EfCoreRepository<CUInventoryDbContext, TEntity, TKey>(dbContextProvider)
    where TEntity : class, IEntity<TKey>
{
    public override async Task<List<TEntity>> GetListAsync(
        bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        using (DataFilter.Enable<IIsActive>())
        {
            return await base.GetListAsync(includeDetails, cancellationToken);
        }
    }

    public override async Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        using (DataFilter.Enable<IIsActive>())
        {
            return await base.GetListAsync(predicate, includeDetails, cancellationToken);
        }
    }

    public override async Task<List<TEntity>> GetPagedListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        bool includeDetails = false,
        CancellationToken cancellationToken = default)
    {
        using (DataFilter.Enable<IIsActive>())
        {
            return await base.GetPagedListAsync(skipCount, maxResultCount, sorting, includeDetails, cancellationToken);
        }
    }

    public override async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        using (DataFilter.Enable<IIsActive>())
        {
            return await base.GetCountAsync(cancellationToken);
        }
    }
}
