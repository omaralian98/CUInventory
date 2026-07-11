using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory.Interfaces;

public interface IInventoryBalanceManager : IDomainService
{
    Task<InventoryBalance> GetOrCreateAsync(Guid warehouseId, Guid productId, decimal? lowStockThreshold = null);
}
