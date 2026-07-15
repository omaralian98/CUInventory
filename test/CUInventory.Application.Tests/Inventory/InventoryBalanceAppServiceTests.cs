using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Dtos;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Inventory;

public abstract class InventoryBalanceAppServiceTests<TStartupModule> : CUInventoryStockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    [Fact]
    public async Task Should_Set_The_Low_Stock_Threshold()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);
        var balanceId = await FindBalanceIdAsync(warehouseId, productId);

        var updated = await InventoryBalanceAppService.SetLowStockThresholdAsync(
            balanceId,
            new SetLowStockThresholdDto { Threshold = 5m, ConcurrencyStamp = (await StampOfBalanceAsync(balanceId)).ConcurrencyStamp });

        updated.LowStockThreshold.ShouldBe(5m);
    }

    [Fact]
    public async Task LowStockOnly_Returns_Balances_Strictly_Below_Their_Threshold()
    {
        var warehouseId = Guid.NewGuid();
        var lowProduct = Guid.NewGuid();
        var okProduct = Guid.NewGuid();
        await SeedStockAsync(warehouseId, lowProduct, quantity: 10m);
        await SeedStockAsync(warehouseId, okProduct, quantity: 10m);

        var lowBalanceId = await FindBalanceIdAsync(warehouseId, lowProduct);
        var okBalanceId = await FindBalanceIdAsync(warehouseId, okProduct);

        // Available (10) is below a threshold of 12 -> low.
        await InventoryBalanceAppService.SetLowStockThresholdAsync(
            lowBalanceId,
            new SetLowStockThresholdDto { Threshold = 12m, ConcurrencyStamp = (await StampOfBalanceAsync(lowBalanceId)).ConcurrencyStamp });
        // Available (10) is above a threshold of 5 -> not low.
        await InventoryBalanceAppService.SetLowStockThresholdAsync(
            okBalanceId,
            new SetLowStockThresholdDto { Threshold = 5m, ConcurrencyStamp = (await StampOfBalanceAsync(okBalanceId)).ConcurrencyStamp });

        var low = await InventoryBalanceAppService.GetListAsync(
            new GetInventoryBalanceListDto { ProductId = lowProduct, LowStockOnly = true });
        low.Items.ShouldHaveSingleItem().Id.ShouldBe(lowBalanceId);

        var ok = await InventoryBalanceAppService.GetListAsync(
            new GetInventoryBalanceListDto { ProductId = okProduct, LowStockOnly = true });
        ok.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task LowStockOnly_Treats_Available_Equal_To_Threshold_As_Not_Low()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);
        var balanceId = await FindBalanceIdAsync(warehouseId, productId);

        // Boundary: available (10) == threshold (10) is NOT below threshold.
        await InventoryBalanceAppService.SetLowStockThresholdAsync(
            balanceId,
            new SetLowStockThresholdDto { Threshold = 10m, ConcurrencyStamp = (await StampOfBalanceAsync(balanceId)).ConcurrencyStamp });

        var low = await InventoryBalanceAppService.GetListAsync(
            new GetInventoryBalanceListDto { ProductId = productId, LowStockOnly = true });
        low.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Filter_Balances_By_Warehouse_And_Product()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 7m);

        var list = await InventoryBalanceAppService.GetListAsync(
            new GetInventoryBalanceListDto { WarehouseId = warehouseId, ProductId = productId });

        var balance = list.Items.Single();
        balance.ShouldSatisfyAllConditions(
            () => balance.WarehouseId.ShouldBe(warehouseId),
            () => balance.ProductId.ShouldBe(productId),
            () => balance.QuantityOnHand.ShouldBe(7m));
    }
}
