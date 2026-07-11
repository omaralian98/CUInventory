using System;
using System.Threading.Tasks;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Repositories;
using CUInventory.Catalog.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Catalog;

public class EfCoreProductRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : EfCoreRepository<CUInventoryDbContext, Product, Guid>(dbContextProvider), IProductRepository
{
    public async Task<Product?> GetProductBySkuOrDefaultAsync(Sku sku)
    {
        var query = await WithDetailsAsync();
        return await query.FirstOrDefaultAsync(
            x => x.SKU != null && x.SKU.Value == sku.Value, GetCancellationToken());
    }
}
