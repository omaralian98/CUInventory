using System;
using System.Threading.Tasks;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Repositories;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Warehousing;

public class EfCoreWarehouseRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : CUInventoryEfCoreRepository<Warehouse, Guid>(dbContextProvider), IWarehouseRepository
{
    public async Task<Warehouse?> GetByCodeOrDefaultAsync(string code)
    {
        var query = await WithDetailsAsync();
        return await query.FirstOrDefaultAsync(x => x.Code == code, GetCancellationToken());
    }
}
