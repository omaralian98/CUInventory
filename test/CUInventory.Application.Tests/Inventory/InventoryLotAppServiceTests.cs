using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Dtos;
using CUInventory.Sales.Dtos;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Inventory;

public abstract class InventoryLotAppServiceTests<TStartupModule> : CUInventoryStockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    [Fact]
    public async Task Receipt_Creates_A_Traceable_Purchase_Lot()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m, unitCost: 5m, supplierId: supplierId);

        var lots = await InventoryLotAppService.GetListAsync(
            new GetInventoryLotListDto { WarehouseId = warehouseId, ProductId = productId });

        var lot = lots.Items.Single();
        lot.ShouldSatisfyAllConditions(
            () => lot.Source.ShouldBe(InventoryLotSource.Purchase),
            () => lot.WarehouseId.ShouldBe(warehouseId),
            () => lot.ProductId.ShouldBe(productId),
            () => lot.SupplierId.ShouldBe(supplierId),
            () => lot.OriginalQuantity.ShouldBe(10m),
            () => lot.RemainingQuantity.ShouldBe(10m),
            () => lot.UnitCost.ShouldBe(5m));
    }

    [Fact]
    public async Task Should_Filter_Lots_By_Supplier()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var supplierA = Guid.NewGuid();
        var supplierB = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 5m, supplierId: supplierA);
        await SeedStockAsync(warehouseId, productId, quantity: 5m, supplierId: supplierB);

        var fromA = await InventoryLotAppService.GetListAsync(
            new GetInventoryLotListDto { WarehouseId = warehouseId, ProductId = productId, SupplierId = supplierA });

        fromA.Items.ShouldHaveSingleItem().SupplierId.ShouldBe(supplierA);
    }

    [Fact]
    public async Task HasRemaining_Excludes_Fully_Consumed_Lots()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 4m);

        var sale = await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 4m, UnitPrice = 20m, WarehouseId = warehouseId } }
        });
        await SaleAppService.ConfirmAsync(sale.Id, await StampOfSaleAsync(sale.Id));

        var all = await InventoryLotAppService.GetListAsync(
            new GetInventoryLotListDto { WarehouseId = warehouseId, ProductId = productId });
        all.Items.ShouldHaveSingleItem().RemainingQuantity.ShouldBe(0m);

        var remaining = await InventoryLotAppService.GetListAsync(
            new GetInventoryLotListDto { WarehouseId = warehouseId, ProductId = productId, HasRemaining = true });
        remaining.TotalCount.ShouldBe(0);
    }
}
