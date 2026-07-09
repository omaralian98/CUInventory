using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Interfaces;

namespace CUInventory.Catalog.Managers;

public class CategoryManager : DomainService, ICategoryManager
{
    public Task<Category> CreateAsync(/* TODO: parameters once Category has properties */)
    {
        throw new NotImplementedException();
    }
}
