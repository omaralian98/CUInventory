using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Warehousing;

public class EfCoreShipmentRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : EfCoreRepository<CUInventoryDbContext, Shipment, Guid>(dbContextProvider), IShipmentRepository
{
    public override async Task<IQueryable<Shipment>> WithDetailsAsync()
    {
        return (await GetQueryableAsync()).Include(x => x.Lines);
    }
}
