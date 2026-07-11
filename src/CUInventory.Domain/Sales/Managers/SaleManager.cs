using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using CUInventory.Inventory;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Allocation;
using CUInventory.Inventory.Exceptions;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Entities;
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
            Reserve(line, balances, candidateLots, now);
        }

        return Task.FromResult(sale);
    }

    public Task<Sale> ConfirmAsync(Sale sale, List<InventoryBalance> balances, List<InventoryLot> candidateLots)
    {
        var now = Clock.Now;
        sale.Confirm(now);

        foreach (var line in sale.Lines)
        {
            var reservations = line.Allocations.Where(a => a.IsReserved).ToList();
            line.ClearAllocations();

            foreach (var reservation in reservations)
            {
                var warehouseLots = candidateLots.Where(l => l.WarehouseId == reservation.WarehouseId).ToList();

                var request = new AllocationRequest(
                    line.ProductId,
                    reservation.Quantity.Value,
                    line.Kind,
                    WarehouseIds: [reservation.WarehouseId],
                    SupplierId: line.SupplierId,
                    LotIds: line.LotId is { } lotId ? [lotId] : null);

                var results = allocationService.Allocate(request, warehouseLots);

                foreach (var result in results)
                {
                    var lot = warehouseLots.First(l => l.Id == result.LotId);
                    lot.Consume(new Quantity(result.Quantity));

                    line.AddAllocation(GuidGenerator.Create(), reservation.WarehouseId, result.LotId, result.SupplierId, result.UnitCost, new Quantity(result.Quantity));

                    var balance = balances.FindRequired(reservation.WarehouseId, line.ProductId);
                    balance.ConfirmReservation(new Quantity(result.Quantity), now);
                }
            }
        }

        return Task.FromResult(sale);
    }

    public Task<Sale> CancelAsync(Sale sale, List<InventoryBalance> balances)
    {
        var now = Clock.Now;
        sale.Cancel();

        foreach (var line in sale.Lines)
        {
            foreach (var reservation in line.Allocations.Where(a => a.IsReserved))
            {
                var balance = balances.FindRequired(reservation.WarehouseId, line.ProductId);
                balance.ReleaseReservation(reservation.Quantity, now);
            }
        }

        return Task.FromResult(sale);
    }

    private void Reserve(
        SaleLine line,
        List<InventoryBalance> balances,
        List<InventoryLot> candidateLots,
        DateTime now)
    {
        var eligibleByWarehouse = EligibleQuantityByWarehouse(line, candidateLots);

        var productBalances = balances.Where(b => b.ProductId == line.ProductId);
        var candidates = line.WarehouseId is { } warehouseId
            ? productBalances.Where(b => b.WarehouseId == warehouseId)
            : productBalances.OrderByDescending(b => ReservableQuantity(b, eligibleByWarehouse));

        var plan = new List<(InventoryBalance Balance, decimal Take)>();
        var remaining = line.Quantity.Value;

        foreach (var balance in candidates)
        {
            if (remaining <= 0)
            {
                break;
            }

            var take = Math.Min(remaining, ReservableQuantity(balance, eligibleByWarehouse));
            if (take <= 0)
            {
                continue;
            }

            plan.Add((balance, take));
            remaining -= take;
        }

        if (remaining > 0)
        {
            throw new InsufficientStockDomainException(line.ProductId, line.Quantity.Value, line.Quantity.Value - remaining);
        }

        foreach (var (balance, take) in plan)
        {
            balance.Reserve(new Quantity(take), now);
            line.AddReservation(GuidGenerator.Create(), balance.WarehouseId, new Quantity(take));
        }
    }

    private static decimal ReservableQuantity(InventoryBalance balance, Dictionary<Guid, decimal>? eligibleByWarehouse)
    {
        if (eligibleByWarehouse is null)
        {
            return balance.QuantityAvailable;
        }

        return eligibleByWarehouse.TryGetValue(balance.WarehouseId, out var eligible)
            ? Math.Min(balance.QuantityAvailable, eligible)
            : 0m;
    }

    private static Dictionary<Guid, decimal>? EligibleQuantityByWarehouse(SaleLine line, List<InventoryLot> candidateLots)
    {
        Func<InventoryLot, bool>? matches = line.Kind switch
        {
            AllocationStrategyKind.SpecificSupplier => lot => lot.SupplierId == line.SupplierId,
            AllocationStrategyKind.SpecificLot => lot => lot.Id == line.LotId,
            _ => null
        };

        if (matches is null)
        {
            return null;
        }

        return candidateLots
            .Where(lot => lot.ProductId == line.ProductId && lot.RemainingQuantity.Value > 0 && matches(lot))
            .GroupBy(lot => lot.WarehouseId)
            .ToDictionary(group => group.Key, group => group.Sum(lot => lot.RemainingQuantity.Value));
    }
}
