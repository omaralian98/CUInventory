using System;
using Volo.Abp.Domain.Repositories;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory.Repositories;

public interface IInventoryBalanceRepository : IRepository<InventoryBalance, Guid>
{
}
