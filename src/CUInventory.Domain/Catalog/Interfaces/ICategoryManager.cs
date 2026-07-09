using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Catalog.Aggregates;

namespace CUInventory.Catalog.Interfaces;

public interface ICategoryManager : IDomainService
{
    Task<Category> CreateAsync(/* TODO: parameters once Category has properties */);
}
