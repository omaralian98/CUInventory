using System;
using Volo.Abp.Domain.Repositories;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory.Repositories;

public interface IStockTransferRepository : IRepository<StockTransfer, Guid>
{
}
