using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Catalog.Aggregates;

namespace CUInventory.Catalog.Interfaces;

public interface IProductManager : IDomainService
{
    Task<Product> CreateAsync(/* TODO: parameters once Product has properties */);
}
