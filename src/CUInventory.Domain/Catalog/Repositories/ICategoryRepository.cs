using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using CUInventory.Catalog.Aggregates;

namespace CUInventory.Catalog.Repositories;

public interface ICategoryRepository : IRepository<Category, Guid>
{
    Task<bool> ExistsAsync(string name);
}
