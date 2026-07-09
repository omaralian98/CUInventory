using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Catalog.Aggregates;

namespace CUInventory.Catalog.Interfaces;

public interface ICategoryManager : IDomainService
{
    Task<Category> CreateAsync(string name);
    Task<Category> UpdateAsync(Category category, string name, int orderIndex, bool isActive);
}
