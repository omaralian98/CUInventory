using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Allocation;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Entities;
using CUInventory.Sales.Exceptions;
using CUInventory.Sales.Interfaces;
using CUInventory.ValueObjects;

namespace CUInventory.Sales.Managers;

public class SaleManager(InventoryAllocationService allocationService) : DomainService, ISaleManager
{
    public Task<Sale> CreateAsync(
        List<SaleLineRequest> lines,
        List<InventoryBalance> balances,
        List<InventoryLot> candidateLots)
    {
        var now = Clock.Now;
        var sale = new Sale(
            GuidGenerator.Create(),
            lines.Select(request => new SaleLineData(
                GuidGenerator.Create(),
                request.ProductId,
                new Quantity(request.Quantity),
                new Money(request.UnitPrice),
                request.Kind,
                request.WarehouseId,
                request.SupplierId,
                request.LotId)).ToList());

        foreach (var line in sale.Lines)
        {
            var results = allocationService.Allocate(ToAllocationRequest(line), CandidatesFor(line, candidateLots));

            foreach (var result in results)
            {
                var lot = candidateLots.First(l => l.Id == result.LotId);
                lot.Reserve(new Quantity(result.Quantity));
                line.AddReservation(
                    GuidGenerator.Create(),
                    result.WarehouseId,
                    result.LotId,
                    result.SupplierId,
                    result.UnitCost,
                    new Quantity(result.Quantity));
            }

            foreach (var group in results.GroupBy(r => r.WarehouseId))
            {
                var balance = balances.FindRequired(group.Key, line.ProductId);
                balance.Reserve(new Quantity(group.Sum(r => r.Quantity)), now);
            }
        }

        return Task.FromResult(sale);
    }

    public Task<Sale> ConfirmAsync(Sale sale, List<InventoryBalance> balances, List<InventoryLot> pinnedLots)
    {
        var now = Clock.Now;
        sale.Confirm(now);

        foreach (var line in sale.Lines)
        {
            foreach (var allocation in line.Allocations.Where(a => a.IsReserved))
            {
                if (allocation.InventoryLotId is not { } lotId)
                {
                    throw new SaleAllocationHasNoLotDomainException(sale.Id, allocation.Id);
                }

                var lot = pinnedLots.First(l => l.Id == lotId);
                lot.ConsumeReserved(allocation.Quantity);
                allocation.MarkConfirmed();

                var balance = balances.FindRequired(allocation.WarehouseId, line.ProductId);
                balance.ConfirmReservation(allocation.Quantity, now);
            }
        }

        return Task.FromResult(sale);
    }

    public Task<Sale> CancelAsync(Sale sale, List<InventoryBalance> balances, List<InventoryLot> pinnedLots)
    {
        var now = Clock.Now;
        sale.Cancel();

        foreach (var line in sale.Lines)
        {
            foreach (var allocation in line.Allocations.Where(a => a.IsReserved))
            {
                if (allocation.InventoryLotId is { } lotId)
                {
                    var lot = pinnedLots.First(l => l.Id == lotId);
                    lot.ReleaseReservation(allocation.Quantity);
                }

                var balance = balances.FindRequired(allocation.WarehouseId, line.ProductId);
                balance.ReleaseReservation(allocation.Quantity, now);
            }
        }

        return Task.FromResult(sale);
    }

    private static AllocationRequest ToAllocationRequest(SaleLine line) =>
        new(
            line.ProductId,
            line.Quantity.Value,
            line.Kind,
            WarehouseIds: line.WarehouseId is { } warehouseId ? [warehouseId] : null,
            SupplierId: line.SupplierId,
            LotIds: line.LotId is { } lotId ? [lotId] : null);

    private static List<InventoryLot> CandidatesFor(SaleLine line, List<InventoryLot> candidateLots) =>
        line.WarehouseId is { } warehouseId
            ? candidateLots.Where(lot => lot.WarehouseId == warehouseId).ToList()
            : candidateLots;
}
