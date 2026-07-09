using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Interfaces;

namespace CUInventory.Inventory.Managers;

public class InventoryBalanceManager : DomainService, IInventoryBalanceManager
{
    public Task<InventoryBalance> CreateAsync(/* TODO: parameters once InventoryBalance has properties */)
    {
        throw new NotImplementedException();
    }
}
