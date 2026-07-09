using System;
using Volo.Abp.Domain.Repositories;
using CUInventory.Catalog.Aggregates;

namespace CUInventory.Catalog.Repositories;

public interface IProductRepository : IRepository<Product, Guid>
{
}
