using System.Linq;
using System.Threading.Tasks;
using CUInventory.Permissions;
using CUInventory.Reporting.Dtos;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;

namespace CUInventory.Reporting;

[Authorize(CUInventoryPermissions.Reports.Default)]
public class ReportsAppService(IReportingRepository reportingRepository) : CUInventoryAppService, IReportsAppService
{
    public virtual async Task<SalesBySourceReportDto> GetSalesBySourceAsync(ReportFilterInput input)
    {
        var report = await reportingRepository.GetSalesBySourceAsync(ToFilter(input));

        var items = report.Items
            .Select(r => new SalesBySourceItemDto
            {
                SupplierId = r.SupplierId,
                ProductId = r.ProductId,
                QuantitySold = r.QuantitySold,
                Revenue = r.Revenue,
                Cost = r.Cost,
                GrossMargin = r.Revenue - r.Cost
            })
            .ToList();

        return new SalesBySourceReportDto
        {
            Items = items,
            TotalCount = report.TotalCount,
            TotalQuantitySold = report.TotalQuantitySold,
            TotalRevenue = report.TotalRevenue,
            TotalCost = report.TotalCost,
            TotalGrossMargin = report.TotalRevenue - report.TotalCost
        };
    }

    public virtual async Task<PagedResultDto<SalesSourceDetailDto>> GetSalesBySourceDetailAsync(ReportFilterInput input)
    {
        var page = await reportingRepository.GetSalesBySourceDetailAsync(ToFilter(input));

        var items = page.Items
            .Select(r => new SalesSourceDetailDto
            {
                SaleId = r.SaleId,
                SaleLineId = r.SaleLineId,
                ProductId = r.ProductId,
                SupplierId = r.SupplierId,
                InventoryLotId = r.InventoryLotId,
                WarehouseId = r.WarehouseId,
                Quantity = r.Quantity,
                UnitPrice = r.UnitPrice,
                UnitCost = r.UnitCost,
                Revenue = r.Quantity * r.UnitPrice,
                Cost = r.Quantity * r.UnitCost,
                GrossMargin = r.Quantity * r.UnitPrice - r.Quantity * r.UnitCost,
                ConfirmedAt = r.ConfirmedAt
            })
            .ToList();

        return new PagedResultDto<SalesSourceDetailDto>(page.TotalCount, items);
    }

    public virtual async Task<RemainingStockReportDto> GetRemainingStockAsync(ReportFilterInput input)
    {
        var report = await reportingRepository.GetRemainingStockAsync(ToFilter(input));

        var items = report.Items
            .Select(r => new RemainingStockItemDto
            {
                WarehouseId = r.WarehouseId,
                SupplierId = r.SupplierId,
                ProductId = r.ProductId,
                RemainingQuantity = r.RemainingQuantity,
                ValueAtCost = r.ValueAtCost
            })
            .ToList();

        return new RemainingStockReportDto
        {
            Items = items,
            TotalCount = report.TotalCount,
            TotalRemainingQuantity = report.TotalRemainingQuantity,
            TotalValueAtCost = report.TotalValueAtCost
        };
    }

    public virtual async Task<PagedResultDto<RemainingStockDetailDto>> GetRemainingStockDetailAsync(ReportFilterInput input)
    {
        var page = await reportingRepository.GetRemainingStockDetailAsync(ToFilter(input));

        var items = page.Items
            .Select(r => new RemainingStockDetailDto
            {
                LotId = r.LotId,
                ProductId = r.ProductId,
                WarehouseId = r.WarehouseId,
                SupplierId = r.SupplierId,
                ShipmentLineId = r.ShipmentLineId,
                Source = r.Source,
                RemainingQuantity = r.RemainingQuantity,
                UnitCost = r.UnitCost,
                ValueAtCost = r.ValueAtCost,
                ReceivedAt = r.ReceivedAt
            })
            .ToList();

        return new PagedResultDto<RemainingStockDetailDto>(page.TotalCount, items);
    }

    public virtual async Task<InventoryValuationReportDto> GetInventoryValuationAsync(ReportFilterInput input)
    {
        var report = await reportingRepository.GetInventoryValuationAsync(ToFilter(input));

        var items = report.Items
            .Select(r => new InventoryValuationItemDto
            {
                WarehouseId = r.WarehouseId,
                CategoryId = r.CategoryId,
                TotalQuantity = r.TotalQuantity,
                TotalValue = r.TotalValue
            })
            .ToList();

        return new InventoryValuationReportDto
        {
            Items = items,
            TotalCount = report.TotalCount,
            GrandTotalQuantity = report.GrandTotalQuantity,
            GrandTotalValue = report.GrandTotalValue
        };
    }

    public virtual async Task<PagedResultDto<LowStockItemDto>> GetLowStockAsync(ReportFilterInput input)
    {
        var page = await reportingRepository.GetLowStockAsync(ToFilter(input));

        var items = page.Items
            .Select(r => new LowStockItemDto
            {
                WarehouseId = r.WarehouseId,
                ProductId = r.ProductId,
                QuantityOnHand = r.QuantityOnHand,
                QuantityReserved = r.QuantityReserved,
                QuantityAvailable = r.QuantityAvailable,
                LowStockThreshold = r.LowStockThreshold
            })
            .ToList();

        return new PagedResultDto<LowStockItemDto>(page.TotalCount, items);
    }

    private static ReportQueryFilter ToFilter(ReportFilterInput input) => new()
    {
        WarehouseId = input.WarehouseId,
        SupplierId = input.SupplierId,
        CategoryId = input.CategoryId,
        ProductId = input.ProductId,
        FromDate = input.FromDate,
        ToDate = input.ToDate,
        SkipCount = input.SkipCount,
        MaxResultCount = input.MaxResultCount,
        Sorting = input.Sorting
    };
}
