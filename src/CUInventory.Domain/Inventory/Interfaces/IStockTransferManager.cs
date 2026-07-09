using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory.Interfaces;

public interface IStockTransferManager : IDomainService
{
    Task<StockTransfer> CreateAsync(/* TODO: parameters once StockTransfer has properties */);
}
