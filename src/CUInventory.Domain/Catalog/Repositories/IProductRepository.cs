using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using CUInventory.Catalog.Aggregates;
using CUInventory.ValueObjects;

namespace CUInventory.Catalog.Repositories;

public interface IProductRepository : IRepository<Product, Guid>
{
    Task<Product?> GetProductBySkuOrDefaultAsync(Sku sku);
}
