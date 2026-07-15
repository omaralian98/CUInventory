using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Allocation;
using CUInventory.Inventory.Interfaces;
using CUInventory.ValueObjects;

namespace CUInventory.Inventory.Managers;

public class StockTransferManager(InventoryAllocationService allocationService)
    : DomainService, IStockTransferManager
{
    public Task<StockTransfer> DispatchAsync(StockTransfer transfer, List<InventoryBalance> sourceBalances, List<InventoryLot> candidateLots)
    {
        var now = Clock.Now;
        transfer.MarkDispatched(now);

        var sourceLots = candidateLots.Where(lot => lot.WarehouseId == transfer.SourceWarehouseId).ToList();

        foreach (var line in transfer.Lines)
        {
            var request = new AllocationRequest(line.ProductId, line.Quantity.Value, AllocationStrategyKind.Fifo);
            var results = allocationService.Allocate(request, sourceLots);

            foreach (var result in results)
            {
                var lot = sourceLots.First(l => l.Id == result.LotId);
                lot.Consume(new Quantity(result.Quantity));

                transfer.AddAllocation(GuidGenerator.Create(), result.LotId, line.ProductId, result.SupplierId, result.UnitCost, new Quantity(result.Quantity));

                var balance = sourceBalances.FindRequired(transfer.SourceWarehouseId, line.ProductId);
                balance.DeductDirect(new Quantity(result.Quantity), now);
            }
        }

        return Task.FromResult(transfer);
    }

    public async Task<List<InventoryLot>> ReceiveAsync(StockTransfer transfer, List<InventoryBalance> destinationBalances, List<InventoryLot> sourceLots)
    {
        var now = Clock.Now;
        transfer.MarkReceived(now);

        var createdLots = new List<InventoryLot>();

        foreach (var allocation in transfer.Allocations)
        {
            var sourceLot = sourceLots.First(l => l.Id == allocation.SourceLotId);
            var lot = new InventoryLot(
                GuidGenerator.Create(),
                allocation.ProductId,
                transfer.DestinationWarehouseId,
                InventoryLotSource.TransferIn,
                allocation.Quantity,
                allocation.UnitCost,
                sourceLot.ReceivedAt,
                allocation.SupplierId,
                sourceLot.ShipmentLineId
            );

            createdLots.Add(lot);

            var balance = destinationBalances.FindRequired(transfer.DestinationWarehouseId, allocation.ProductId);
            balance.Increase(allocation.Quantity, now);
        }

        return createdLots;
    }

    public Task<StockTransfer> CancelAsync(StockTransfer transfer, List<InventoryBalance> sourceBalances, List<InventoryLot> sourceLots)
    {
        if (transfer.Status == StockTransferStatus.Dispatched)
        {
            var now = Clock.Now;

            foreach (var allocation in transfer.Allocations)
            {
                var lot = sourceLots.First(l => l.Id == allocation.SourceLotId);
                lot.Restore(allocation.Quantity);

                var balance = sourceBalances.FindRequired(transfer.SourceWarehouseId, allocation.ProductId);
                balance.Increase(allocation.Quantity, now);
            }
        }

        transfer.MarkCancelled();
        return Task.FromResult(transfer);
    }
}
