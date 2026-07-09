using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory.Interfaces;

public interface IInventoryLotManager : IDomainService
{
    Task<InventoryLot> CreateAsync(/* TODO: parameters once InventoryLot has properties */);
}
