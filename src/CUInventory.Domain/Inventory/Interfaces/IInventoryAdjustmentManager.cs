using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory.Interfaces;

public interface IInventoryAdjustmentManager : IDomainService
{
    Task<InventoryAdjustment> CreateAsync(/* TODO: parameters once InventoryAdjustment has properties */);
}
