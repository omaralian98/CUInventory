using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;

namespace CUInventory.Warehousing.Interfaces;

public interface IWarehouseManager : IDomainService
{
    Task<Warehouse> CreateAsync(string name, string code, Address address);
    Task<Warehouse> UpdateAsync(Warehouse warehouse, string name, string code, Address address, bool isActive);
}
