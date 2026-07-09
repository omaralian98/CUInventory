using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Warehousing.Aggregates;

namespace CUInventory.Warehousing.Interfaces;

public interface IWarehouseManager : IDomainService
{
    Task<Warehouse> CreateAsync(/* TODO: parameters once Warehouse has properties */);
}
