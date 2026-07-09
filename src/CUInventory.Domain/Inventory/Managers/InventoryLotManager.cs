using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Interfaces;

namespace CUInventory.Inventory.Managers;

public class InventoryLotManager : DomainService, IInventoryLotManager
{
    public Task<InventoryLot> CreateAsync(/* TODO: parameters once InventoryLot has properties */)
    {
        throw new NotImplementedException();
    }
}
