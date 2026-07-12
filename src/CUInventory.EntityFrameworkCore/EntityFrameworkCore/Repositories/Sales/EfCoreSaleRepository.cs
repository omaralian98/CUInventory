using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Sales;

public class EfCoreSaleRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : CUInventoryEfCoreRepository<Sale, Guid>(dbContextProvider), ISaleRepository
{
    public override async Task<IQueryable<Sale>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(x => x.Lines)
            .ThenInclude(l => l.Allocations);
    }
}
