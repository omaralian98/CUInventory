using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Sales.Aggregates;

namespace CUInventory.Sales.Interfaces;

public interface ISaleManager : IDomainService
{
    Task<Sale> CreateAsync(/* TODO: parameters once Sale has properties */);
}
