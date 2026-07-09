using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Catalog.Aggregates;
using CUInventory.ValueObjects;

namespace CUInventory.Catalog.Interfaces;

public interface IProductManager : IDomainService
{
    Task<Product> CreateAsync(string name, string? description, Sku? sku, bool isService = false, Guid? categoryId = null);
    Task<Product> UpdateAsync(Product product, string name, string? description, Sku? sku, bool isService = false, Guid? categoryId = null);
    Task<Product> SetSkuAsync(Product product, Sku? sku);
}
