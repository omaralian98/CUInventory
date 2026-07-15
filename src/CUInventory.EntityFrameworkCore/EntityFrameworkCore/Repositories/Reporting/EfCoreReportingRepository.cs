using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CUInventory.Abstractions;
using CUInventory.Catalog.Aggregates;
using CUInventory.Inventory;
using CUInventory.Inventory.Aggregates;
using CUInventory.Reporting;
using CUInventory.Sales;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;

namespace CUInventory.EntityFrameworkCore.Repositories.Reporting;

[ExposeServices(typeof(IReportingRepository))]
public class EfCoreReportingRepository(
    IDbContextProvider<CUInventoryDbContext> dbContextProvider,
    IDataFilter dataFilter)
    : IReportingRepository, ITransientDependency
{
    // Reporting spans historical data, so a product/warehouse deactivated after the fact must still
    // count. Soft-delete and multi-tenant filters stay on; only the IsActive filter is suppressed.
    private IDisposable IncludeInactive() => dataFilter.Disable<IIsActive>();

    public async Task<SalesBySourceReport> GetSalesBySourceAsync(
        ReportQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var db = await dbContextProvider.GetDbContextAsync();
        using (IncludeInactive())
        {
            var flat = BuildSalesQuery(db, filter);
            var grouped = flat.GroupBy(x => new { x.SupplierId, x.ProductId });

            var totalCount = await grouped.LongCountAsync(cancellationToken);

            var items = await grouped
                .OrderBy(g => g.Key.ProductId)
                .ThenBy(g => g.Key.SupplierId)
                .Skip(filter.SkipCount)
                .Take(filter.MaxResultCount)
                .Select(g => new SalesBySourceRow(
                    g.Key.SupplierId,
                    g.Key.ProductId,
                    g.Sum(x => x.Quantity),
                    g.Sum(x => x.Quantity * x.UnitPrice),
                    g.Sum(x => x.Quantity * x.UnitCost)))
                .ToListAsync(cancellationToken);

            var totalQuantity = await flat.SumAsync(x => (decimal?)x.Quantity, cancellationToken) ?? 0m;
            var totalRevenue = await flat.SumAsync(x => (decimal?)(x.Quantity * x.UnitPrice), cancellationToken) ?? 0m;
            var totalCost = await flat.SumAsync(x => (decimal?)(x.Quantity * x.UnitCost), cancellationToken) ?? 0m;

            return new SalesBySourceReport(items, totalCount, totalQuantity, totalRevenue, totalCost);
        }
    }

    public async Task<ReportPage<SalesBySourceDetailRow>> GetSalesBySourceDetailAsync(
        ReportQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var db = await dbContextProvider.GetDbContextAsync();
        using (IncludeInactive())
        {
            var query = BuildSalesQuery(db, filter);
            var totalCount = await query.LongCountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.ConfirmedAt)
                .ThenBy(x => x.SaleId)
                .Skip(filter.SkipCount)
                .Take(filter.MaxResultCount)
                .Select(x => new SalesBySourceDetailRow(
                    x.SaleId,
                    x.SaleLineId,
                    x.ProductId,
                    x.SupplierId,
                    x.InventoryLotId,
                    x.WarehouseId,
                    x.Quantity,
                    x.UnitPrice,
                    x.UnitCost,
                    x.ConfirmedAt))
                .ToListAsync(cancellationToken);

            return new ReportPage<SalesBySourceDetailRow>(items, totalCount);
        }
    }

    public async Task<RemainingStockReport> GetRemainingStockAsync(
        ReportQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var db = await dbContextProvider.GetDbContextAsync();
        using (IncludeInactive())
        {
            var lots = BuildLotsQuery(db, filter);
            var grouped = lots.GroupBy(x => new { x.WarehouseId, x.SupplierId, x.ProductId });

            var totalCount = await grouped.LongCountAsync(cancellationToken);

            var items = await grouped
                .OrderBy(g => g.Key.WarehouseId)
                .ThenBy(g => g.Key.ProductId)
                .ThenBy(g => g.Key.SupplierId)
                .Skip(filter.SkipCount)
                .Take(filter.MaxResultCount)
                .Select(g => new RemainingStockRow(
                    g.Key.WarehouseId,
                    g.Key.SupplierId,
                    g.Key.ProductId,
                    g.Sum(x => x.RemainingQuantity),
                    g.Sum(x => x.RemainingQuantity * x.UnitCost)))
                .ToListAsync(cancellationToken);

            var totalRemaining = await lots.SumAsync(x => (decimal?)x.RemainingQuantity, cancellationToken) ?? 0m;
            var totalValue = await lots.SumAsync(x => (decimal?)(x.RemainingQuantity * x.UnitCost), cancellationToken) ?? 0m;

            return new RemainingStockReport(items, totalCount, totalRemaining, totalValue);
        }
    }

    public async Task<ReportPage<RemainingStockDetailRow>> GetRemainingStockDetailAsync(
        ReportQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var db = await dbContextProvider.GetDbContextAsync();
        using (IncludeInactive())
        {
            var query = BuildLotsQuery(db, filter);
            var totalCount = await query.LongCountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.ReceivedAt)
                .ThenBy(x => x.LotId)
                .Skip(filter.SkipCount)
                .Take(filter.MaxResultCount)
                .Select(x => new RemainingStockDetailRow(
                    x.LotId,
                    x.ProductId,
                    x.WarehouseId,
                    x.SupplierId,
                    x.ShipmentLineId,
                    x.Source,
                    x.RemainingQuantity,
                    x.UnitCost,
                    x.RemainingQuantity * x.UnitCost,
                    x.ReceivedAt))
                .ToListAsync(cancellationToken);

            return new ReportPage<RemainingStockDetailRow>(items, totalCount);
        }
    }

    public async Task<InventoryValuationReport> GetInventoryValuationAsync(
        ReportQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var db = await dbContextProvider.GetDbContextAsync();
        using (IncludeInactive())
        {
            var lots = BuildLotsQuery(db, filter);
            var grouped = lots.GroupBy(x => new { x.WarehouseId, x.CategoryId });

            var totalCount = await grouped.LongCountAsync(cancellationToken);

            var items = await grouped
                .OrderBy(g => g.Key.WarehouseId)
                .ThenBy(g => g.Key.CategoryId)
                .Skip(filter.SkipCount)
                .Take(filter.MaxResultCount)
                .Select(g => new InventoryValuationRow(
                    g.Key.WarehouseId,
                    g.Key.CategoryId,
                    g.Sum(x => x.RemainingQuantity),
                    g.Sum(x => x.RemainingQuantity * x.UnitCost)))
                .ToListAsync(cancellationToken);

            var grandQuantity = await lots.SumAsync(x => (decimal?)x.RemainingQuantity, cancellationToken) ?? 0m;
            var grandValue = await lots.SumAsync(x => (decimal?)(x.RemainingQuantity * x.UnitCost), cancellationToken) ?? 0m;

            return new InventoryValuationReport(items, totalCount, grandQuantity, grandValue);
        }
    }

    public async Task<ReportPage<LowStockRow>> GetLowStockAsync(
        ReportQueryFilter filter,
        CancellationToken cancellationToken = default)
    {
        var db = await dbContextProvider.GetDbContextAsync();
        using (IncludeInactive())
        {
            var query =
                from b in db.Set<InventoryBalance>()
                join p in db.Set<Product>() on b.ProductId equals p.Id
                where b.LowStockThreshold != null
                      && (b.QuantityOnHand - b.QuantityReserved) <= b.LowStockThreshold
                select new LowStockFlat
                {
                    WarehouseId = b.WarehouseId,
                    ProductId = b.ProductId,
                    CategoryId = p.CategoryId,
                    QuantityOnHand = b.QuantityOnHand,
                    QuantityReserved = b.QuantityReserved,
                    LowStockThreshold = b.LowStockThreshold,
                    BalanceId = b.Id
                };

            if (filter.WarehouseId is { } warehouseId)
            {
                query = query.Where(x => x.WarehouseId == warehouseId);
            }

            if (filter.ProductId is { } productId)
            {
                query = query.Where(x => x.ProductId == productId);
            }

            if (filter.CategoryId is { } categoryId)
            {
                query = query.Where(x => x.CategoryId == categoryId);
            }

            var totalCount = await query.LongCountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.QuantityOnHand - x.QuantityReserved)
                .ThenBy(x => x.BalanceId)
                .Skip(filter.SkipCount)
                .Take(filter.MaxResultCount)
                .Select(x => new LowStockRow(
                    x.WarehouseId,
                    x.ProductId,
                    x.QuantityOnHand,
                    x.QuantityReserved,
                    x.QuantityOnHand - x.QuantityReserved,
                    x.LowStockThreshold))
                .ToListAsync(cancellationToken);

            return new ReportPage<LowStockRow>(items, totalCount);
        }
    }

    // Flatten the Sale -> SaleLine -> SaleAllocation -> Product chain for confirmed, lot-bound
    // allocations into primitive columns. Member-init projections (unlike positional-record ones)
    // compose with GroupBy, so callers can aggregate or page over the same query shape.
    private static IQueryable<SalesFlat> BuildSalesQuery(CUInventoryDbContext db, ReportQueryFilter filter)
    {
        var query =
            from a in db.Set<SaleAllocation>()
            join sl in db.Set<SaleLine>() on a.SaleLineId equals sl.Id
            join s in db.Set<Sale>() on sl.SaleId equals s.Id
            join p in db.Set<Product>() on sl.ProductId equals p.Id
            where s.Status == SaleStatus.Confirmed && a.InventoryLotId != null
            select new SalesFlat
            {
                SaleId = s.Id,
                SaleLineId = sl.Id,
                ProductId = sl.ProductId,
                SupplierId = a.SupplierId,
                InventoryLotId = a.InventoryLotId,
                WarehouseId = a.WarehouseId,
                CategoryId = p.CategoryId,
                Quantity = a.Quantity.Value,
                UnitPrice = sl.UnitPrice.Amount,
                UnitCost = a.UnitCost!.Amount,
                ConfirmedAt = s.ConfirmedAt
            };

        if (filter.WarehouseId is { } warehouseId)
        {
            query = query.Where(x => x.WarehouseId == warehouseId);
        }

        if (filter.SupplierId is { } supplierId)
        {
            query = query.Where(x => x.SupplierId == supplierId);
        }

        if (filter.ProductId is { } productId)
        {
            query = query.Where(x => x.ProductId == productId);
        }

        if (filter.CategoryId is { } categoryId)
        {
            query = query.Where(x => x.CategoryId == categoryId);
        }

        if (filter.FromDate is { } fromDate)
        {
            query = query.Where(x => x.ConfirmedAt >= fromDate);
        }

        if (filter.ToDate is { } toDate)
        {
            query = query.Where(x => x.ConfirmedAt <= toDate);
        }

        return query;
    }

    private static IQueryable<LotFlat> BuildLotsQuery(CUInventoryDbContext db, ReportQueryFilter filter)
    {
        var query =
            from l in db.Set<InventoryLot>()
            join p in db.Set<Product>() on l.ProductId equals p.Id
            where l.RemainingQuantity.Value > 0
            select new LotFlat
            {
                LotId = l.Id,
                ProductId = l.ProductId,
                WarehouseId = l.WarehouseId,
                SupplierId = l.SupplierId,
                CategoryId = p.CategoryId,
                ShipmentLineId = l.ShipmentLineId,
                Source = l.Source,
                RemainingQuantity = l.RemainingQuantity.Value,
                UnitCost = l.UnitCost.Amount,
                ReceivedAt = l.ReceivedAt
            };

        if (filter.WarehouseId is { } warehouseId)
        {
            query = query.Where(x => x.WarehouseId == warehouseId);
        }

        if (filter.SupplierId is { } supplierId)
        {
            query = query.Where(x => x.SupplierId == supplierId);
        }

        if (filter.ProductId is { } productId)
        {
            query = query.Where(x => x.ProductId == productId);
        }

        if (filter.CategoryId is { } categoryId)
        {
            query = query.Where(x => x.CategoryId == categoryId);
        }

        if (filter.FromDate is { } fromDate)
        {
            query = query.Where(x => x.ReceivedAt >= fromDate);
        }

        if (filter.ToDate is { } toDate)
        {
            query = query.Where(x => x.ReceivedAt <= toDate);
        }

        return query;
    }

    private sealed class SalesFlat
    {
        public Guid SaleId { get; set; }
        public Guid SaleLineId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? InventoryLotId { get; set; }
        public Guid WarehouseId { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }

    private sealed class LotFlat
    {
        public Guid LotId { get; set; }
        public Guid ProductId { get; set; }
        public Guid WarehouseId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? ShipmentLineId { get; set; }
        public InventoryLotSource Source { get; set; }
        public decimal RemainingQuantity { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime ReceivedAt { get; set; }
    }

    private sealed class LowStockFlat
    {
        public Guid WarehouseId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal QuantityOnHand { get; set; }
        public decimal QuantityReserved { get; set; }
        public decimal? LowStockThreshold { get; set; }
        public Guid BalanceId { get; set; }
    }
}
