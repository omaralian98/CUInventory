using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Interfaces;

namespace CUInventory.Sales.Managers;

public class SaleManager : DomainService, ISaleManager
{
    public Task<Sale> CreateAsync(/* TODO: parameters once Sale has properties */)
    {
        throw new NotImplementedException();
    }
}
