using System;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Repositories;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Inventory;

public class EfCoreInventoryLotRepository(IDbContextProvider<CUInventoryDbContext> dbContextProvider)
    : EfCoreRepository<CUInventoryDbContext, InventoryLot, Guid>(dbContextProvider), IInventoryLotRepository
{
}
