using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Interfaces;

namespace CUInventory.Catalog.Managers;

public class ProductManager : DomainService, IProductManager
{
    public Task<Product> CreateAsync(/* TODO: parameters once Product has properties */)
    {
        throw new NotImplementedException();
    }
}
