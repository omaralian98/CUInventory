using System;
using Volo.Abp.Domain.Repositories;
using CUInventory.Warehousing.Aggregates;

namespace CUInventory.Warehousing.Repositories;

public interface IWarehouseRepository : IRepository<Warehouse, Guid>
{
}
