using System;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Repositories;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Inventory;

public class EfCoreInventoryLotRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : CUInventoryEfCoreRepository<InventoryLot, Guid>(dbContextProvider), IInventoryLotRepository
{
}
