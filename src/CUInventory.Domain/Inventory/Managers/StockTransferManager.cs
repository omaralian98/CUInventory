using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Interfaces;

namespace CUInventory.Inventory.Managers;

public class StockTransferManager : DomainService, IStockTransferManager
{
    public Task<StockTransfer> CreateAsync(/* TODO: parameters once StockTransfer has properties */)
    {
        throw new NotImplementedException();
    }
}
