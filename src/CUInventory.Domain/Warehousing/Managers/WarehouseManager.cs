using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Interfaces;

namespace CUInventory.Warehousing.Managers;

public class WarehouseManager : DomainService, IWarehouseManager
{
    public Task<Warehouse> CreateAsync(/* TODO: parameters once Warehouse has properties */)
    {
        throw new NotImplementedException();
    }
}
