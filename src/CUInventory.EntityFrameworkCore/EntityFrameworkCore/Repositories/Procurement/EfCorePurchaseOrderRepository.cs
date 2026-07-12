using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Procurement;

public class EfCorePurchaseOrderRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : CUInventoryEfCoreRepository<PurchaseOrder, Guid>(dbContextProvider), IPurchaseOrderRepository
{
    public override async Task<IQueryable<PurchaseOrder>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).Include(x => x.Lines);
    }
}
