using System;
using Volo.Abp.Domain.Repositories;
using CUInventory.Catalog.Aggregates;

namespace CUInventory.Catalog.Repositories;

public interface ICategoryRepository : IRepository<Category, Guid>
{
}
