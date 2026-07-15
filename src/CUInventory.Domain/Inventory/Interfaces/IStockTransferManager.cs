using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;

namespace CUInventory.Inventory.Interfaces;

public interface IStockTransferManager : IDomainService
{
    Task<StockTransfer> DispatchAsync(StockTransfer transfer, List<InventoryBalance> sourceBalances, List<InventoryLot> candidateLots);
    Task<List<InventoryLot>> ReceiveAsync(StockTransfer transfer, List<InventoryBalance> destinationBalances, List<InventoryLot> sourceLots);
    Task<StockTransfer> CancelAsync(StockTransfer transfer, List<InventoryBalance> sourceBalances, List<InventoryLot> sourceLots);
}
