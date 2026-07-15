using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Catalog;
using CUInventory.Catalog.Dtos;
using CUInventory.Inventory;
using CUInventory.Inventory.Dtos;
using CUInventory.Reporting.Dtos;
using CUInventory.Sales.Dtos;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Reporting;

public abstract class ReportsAppServiceTests<TStartupModule> : CUInventoryStockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IReportsAppService _reports;
    private readonly ICategoryAppService _categoryAppService;

    protected ReportsAppServiceTests()
    {
        _reports = GetRequiredService<IReportsAppService>();
        _categoryAppService = GetRequiredService<ICategoryAppService>();
    }

    private async Task SellFromSupplierAsync(Guid warehouseId, Guid productId, Guid supplierId, decimal quantity, decimal unitPrice)
    {
        var sale = await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines =
            {
                new CreateSaleLineDto
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    WarehouseId = warehouseId,
                    Kind = AllocationStrategyKind.SpecificSupplier,
                    SupplierId = supplierId
                }
            }
        });
        await SaleAppService.ConfirmAsync(sale.Id, await StampOfSaleAsync(sale.Id));
    }

    [Fact]
    public async Task SalesBySource_Attributes_Sold_Quantities_To_Their_Originating_Supplier()
    {
        var warehouseId = await SeedWarehouseAsync();
        var productId = await SeedProductAsync();
        var supplierA = Guid.NewGuid();
        var supplierB = Guid.NewGuid();

        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierA);
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 7m, supplierId: supplierB);

        await SellFromSupplierAsync(warehouseId, productId, supplierA, quantity: 4m, unitPrice: 20m);
        await SellFromSupplierAsync(warehouseId, productId, supplierB, quantity: 3m, unitPrice: 20m);

        var report = await _reports.GetSalesBySourceAsync(new ReportFilterInput { ProductId = productId });

        var fromA = report.Items.Single(i => i.SupplierId == supplierA);
        var fromB = report.Items.Single(i => i.SupplierId == supplierB);

        fromA.ShouldSatisfyAllConditions(
            () => fromA.QuantitySold.ShouldBe(4m),
            () => fromA.Revenue.ShouldBe(80m),
            () => fromA.Cost.ShouldBe(20m),
            () => fromA.GrossMargin.ShouldBe(60m));

        fromB.ShouldSatisfyAllConditions(
            () => fromB.QuantitySold.ShouldBe(3m),
            () => fromB.Revenue.ShouldBe(60m),
            () => fromB.Cost.ShouldBe(21m),
            () => fromB.GrossMargin.ShouldBe(39m));

        report.ShouldSatisfyAllConditions(
            () => report.TotalCount.ShouldBe(2),
            () => report.TotalQuantitySold.ShouldBe(7m),
            () => report.TotalRevenue.ShouldBe(140m),
            () => report.TotalCost.ShouldBe(41m),
            () => report.TotalGrossMargin.ShouldBe(99m));
    }

    [Fact]
    public async Task SalesBySource_Pages_Groups_While_Totals_Span_The_Whole_Result()
    {
        var warehouseId = await SeedWarehouseAsync();
        var productId = await SeedProductAsync();
        var supplierA = Guid.NewGuid();
        var supplierB = Guid.NewGuid();
        var supplierC = Guid.NewGuid();

        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierA);
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierB);
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierC);
        await SellFromSupplierAsync(warehouseId, productId, supplierA, quantity: 2m, unitPrice: 20m);
        await SellFromSupplierAsync(warehouseId, productId, supplierB, quantity: 2m, unitPrice: 20m);
        await SellFromSupplierAsync(warehouseId, productId, supplierC, quantity: 2m, unitPrice: 20m);

        var firstPage = await _reports.GetSalesBySourceAsync(
            new ReportFilterInput { ProductId = productId, SkipCount = 0, MaxResultCount = 2 });

        firstPage.ShouldSatisfyAllConditions(
            () => firstPage.Items.Count.ShouldBe(2),
            () => firstPage.TotalCount.ShouldBe(3),
            // Grand totals reflect all three groups, not just the two on this page.
            () => firstPage.TotalQuantitySold.ShouldBe(6m),
            () => firstPage.TotalRevenue.ShouldBe(120m));

        var secondPage = await _reports.GetSalesBySourceAsync(
            new ReportFilterInput { ProductId = productId, SkipCount = 2, MaxResultCount = 2 });

        secondPage.ShouldSatisfyAllConditions(
            () => secondPage.Items.Count.ShouldBe(1),
            () => secondPage.TotalCount.ShouldBe(3));
    }

    [Fact]
    public async Task SalesBySource_Can_Filter_To_A_Single_Supplier()
    {
        var warehouseId = await SeedWarehouseAsync();
        var productId = await SeedProductAsync();
        var supplierA = Guid.NewGuid();
        var supplierB = Guid.NewGuid();

        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierA);
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 7m, supplierId: supplierB);
        await SellFromSupplierAsync(warehouseId, productId, supplierA, quantity: 4m, unitPrice: 20m);
        await SellFromSupplierAsync(warehouseId, productId, supplierB, quantity: 3m, unitPrice: 20m);

        var report = await _reports.GetSalesBySourceAsync(
            new ReportFilterInput { ProductId = productId, SupplierId = supplierA });

        report.Items.ShouldHaveSingleItem();
        report.Items[0].SupplierId.ShouldBe(supplierA);
        report.Items[0].QuantitySold.ShouldBe(4m);
    }

    [Fact]
    public async Task SalesBySource_Respects_The_Date_Range()
    {
        var warehouseId = await SeedWarehouseAsync();
        var productId = await SeedProductAsync();
        var supplierId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierId);
        await SellFromSupplierAsync(warehouseId, productId, supplierId, quantity: 4m, unitPrice: 20m);

        var future = await _reports.GetSalesBySourceAsync(
            new ReportFilterInput { ProductId = productId, FromDate = DateTime.Now.AddYears(1) });
        future.Items.ShouldBeEmpty();

        var window = await _reports.GetSalesBySourceAsync(new ReportFilterInput
        {
            ProductId = productId,
            FromDate = DateTime.Now.AddYears(-1),
            ToDate = DateTime.Now.AddYears(1)
        });
        window.Items.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task SalesBySource_Detail_Returns_Per_Allocation_Rows()
    {
        var warehouseId = await SeedWarehouseAsync();
        var productId = await SeedProductAsync();
        var supplierId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierId);
        await SellFromSupplierAsync(warehouseId, productId, supplierId, quantity: 4m, unitPrice: 20m);

        var detail = await _reports.GetSalesBySourceDetailAsync(new ReportFilterInput { ProductId = productId });

        detail.TotalCount.ShouldBe(1);
        var row = detail.Items[0];
        row.ShouldSatisfyAllConditions(
            () => row.SupplierId.ShouldBe(supplierId),
            () => row.InventoryLotId.ShouldNotBeNull(),
            () => row.Quantity.ShouldBe(4m),
            () => row.Revenue.ShouldBe(80m),
            () => row.Cost.ShouldBe(20m),
            () => row.GrossMargin.ShouldBe(60m));
    }

    [Fact]
    public async Task RemainingStock_Reports_On_Hand_Value_And_Honors_The_Received_Date_Window()
    {
        var warehouseId = await SeedWarehouseAsync();
        var productId = await SeedProductAsync();
        var supplierId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierId);
        await SellFromSupplierAsync(warehouseId, productId, supplierId, quantity: 4m, unitPrice: 20m);

        var report = await _reports.GetRemainingStockAsync(new ReportFilterInput { ProductId = productId });
        var row = report.Items.Single();
        row.ShouldSatisfyAllConditions(
            () => row.RemainingQuantity.ShouldBe(6m),
            () => row.ValueAtCost.ShouldBe(30m),
            () => row.SupplierId.ShouldBe(supplierId));
        report.TotalValueAtCost.ShouldBe(30m);

        // "received before our window" -> excluded.
        var past = await _reports.GetRemainingStockAsync(
            new ReportFilterInput { ProductId = productId, ToDate = DateTime.Now.AddYears(-1) });
        past.Items.ShouldBeEmpty();

        // A window covering the receipt (the "received last March" style query) -> included.
        var covered = await _reports.GetRemainingStockAsync(new ReportFilterInput
        {
            ProductId = productId,
            FromDate = DateTime.Now.AddYears(-1),
            ToDate = DateTime.Now.AddYears(1)
        });
        covered.Items.ShouldHaveSingleItem();
    }

    [Fact]
    public async Task InventoryValuation_Totals_On_Hand_Value()
    {
        var warehouseId = await SeedWarehouseAsync();
        var productId = await SeedProductAsync();
        var supplierId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierId);

        var report = await _reports.GetInventoryValuationAsync(new ReportFilterInput { WarehouseId = warehouseId });

        report.GrandTotalQuantity.ShouldBe(10m);
        report.GrandTotalValue.ShouldBe(50m);
    }

    [Fact]
    public async Task LowStock_Lists_Balances_At_Or_Below_Their_Threshold()
    {
        var warehouseId = await SeedWarehouseAsync();
        var productId = await SeedProductAsync();
        var supplierId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierId);

        var balanceId = await FindBalanceIdAsync(warehouseId, productId);
        await InventoryBalanceAppService.SetLowStockThresholdAsync(balanceId, new SetLowStockThresholdDto
        {
            Threshold = 20m,
            ConcurrencyStamp = (await StampOfBalanceAsync(balanceId)).ConcurrencyStamp
        });

        var report = await _reports.GetLowStockAsync(new ReportFilterInput { ProductId = productId });

        report.TotalCount.ShouldBe(1);
        report.Items[0].ShouldSatisfyAllConditions(
            () => report.Items[0].QuantityAvailable.ShouldBe(10m),
            () => report.Items[0].LowStockThreshold.ShouldBe(20m));
    }

    [Fact]
    public async Task LowStock_Excludes_Balances_Above_Their_Threshold()
    {
        var warehouseId = await SeedWarehouseAsync();
        var productId = await SeedProductAsync();
        var supplierId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierId);

        var balanceId = await FindBalanceIdAsync(warehouseId, productId);
        await InventoryBalanceAppService.SetLowStockThresholdAsync(balanceId, new SetLowStockThresholdDto
        {
            Threshold = 5m,
            ConcurrencyStamp = (await StampOfBalanceAsync(balanceId)).ConcurrencyStamp
        });

        var report = await _reports.GetLowStockAsync(new ReportFilterInput { ProductId = productId });

        report.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task SalesBySource_Filters_By_Category_And_Still_Counts_Deactivated_Products()
    {
        var category = await _categoryAppService.CreateAsync(new CreateCategoryDto { Name = $"Cat-{Guid.NewGuid():N}" });
        var warehouseId = await SeedWarehouseAsync();
        var categorizedProductId = await SeedProductAsync(categoryId: category.Id);
        var otherProductId = await SeedProductAsync();
        var supplierId = Guid.NewGuid();

        await SeedStockAsync(warehouseId, categorizedProductId, quantity: 10m, unitCost: 5m, supplierId: supplierId);
        await SeedStockAsync(warehouseId, otherProductId, quantity: 10m, unitCost: 5m, supplierId: supplierId);
        await SellFromSupplierAsync(warehouseId, categorizedProductId, supplierId, quantity: 4m, unitPrice: 20m);
        await SellFromSupplierAsync(warehouseId, otherProductId, supplierId, quantity: 2m, unitPrice: 20m);

        // Deactivate the categorized product after the sale; historical reporting must still count it.
        var product = await ProductAppService.GetAsync(categorizedProductId);
        await ProductAppService.UpdateAsync(categorizedProductId, new UpdateProductDto
        {
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            IsService = product.IsService,
            CategoryId = product.CategoryId,
            IsActive = false,
            OrderIndex = product.OrderIndex,
            ConcurrencyStamp = product.ConcurrencyStamp
        });

        var report = await _reports.GetSalesBySourceAsync(new ReportFilterInput { CategoryId = category.Id });

        report.Items.ShouldHaveSingleItem();
        report.Items[0].ProductId.ShouldBe(categorizedProductId);
        report.Items[0].QuantitySold.ShouldBe(4m);
    }
}
