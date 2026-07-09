using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Interfaces;

namespace CUInventory.Inventory.Managers;

public class InventoryAdjustmentManager : DomainService, IInventoryAdjustmentManager
{
    public Task<InventoryAdjustment> CreateAsync(/* TODO: parameters once InventoryAdjustment has properties */)
    {
        throw new NotImplementedException();
    }
}
