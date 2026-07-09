using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Exceptions;
using CUInventory.Catalog.Interfaces;
using CUInventory.Catalog.Repositories;
using CUInventory.Catalog.ValueObjects;
using CUInventory.ValueObjects;

namespace CUInventory.Catalog.Managers;

public class ProductManager(IProductRepository productRepository) : DomainService, IProductManager
{
    public Task<Product> CreateAsync(string name, string? description, Sku? sku, bool isService = false, Guid? categoryId = null)
    {
        var product = new Product(GuidGenerator.Create(), name, description, sku: null, isService, categoryId);
        return SetSkuAsync(product, sku);
    }

    public async Task<Product> UpdateAsync(Product product, string name, string? description, Sku? sku, bool isService = false, Guid? categoryId = null)
    {
        if (string.Equals(product.Name, name) == false)
        {
            product.SetName(name);
        }

        if (string.Equals(product.Description, description) == false)
        {
            product.SetDescription(description);
        }

        if (product.SKU != sku)
        {
            if (sku is null)
            {
                product.SetSku(sku);
            }
            else
            {
                product = await SetSkuAsync(product, sku);
            }
        }

        product.SetIsService(isService);
        product.SetCategory(categoryId);

        return product;
    }

    public async Task<Product> SetSkuAsync(Product product, Sku? sku)
    {
        if (product.SKU == sku)
        {
            return product;
        }

        if (sku is not null)
        {
            var exists = await productRepository.GetProductBySkuOrDefaultAsync(sku);
            if (exists is not null)
            {
                throw new ProductSkuAlreadyExistsDomainException(sku);
            }
        }

        product.SetSku(sku);
        return product;
    }
}
