using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using CUInventory.Warehousing.Aggregates;

namespace CUInventory.Warehousing.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse, Guid>
{
    Task<Warehouse?> GetByCodeOrDefaultAsync(string code);
}
