using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Dtos;
using CUInventory.Inventory.RealTime;
using CUInventory.Sales.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Inventory.RealTime;

public abstract class StockNotificationTests<TStartupModule> : CUInventoryStockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IStockNotificationBroadcaster _broadcaster;

    protected StockNotificationTests()
    {
        _broadcaster = GetRequiredService<IStockNotificationBroadcaster>();
    }

    [Fact]
    public async Task Receiving_Stock_Publishes_A_StockChanged_Notification()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        using var subscription = _broadcaster.Subscribe();

        await SeedStockAsync(warehouseId, productId, quantity: 10m);

        var notifications = await subscription.DrainAsync();

        var stockChanged = notifications
            .Where(n => n.ProductId == productId && n.Type == StockNotificationType.StockChanged)
            .ToList();
        stockChanged.ShouldNotBeEmpty();

        var latest = stockChanged.Last();
        latest.ShouldSatisfyAllConditions(
            () => latest.WarehouseId.ShouldBe(warehouseId),
            () => latest.QuantityOnHand.ShouldBe(10m),
            () => latest.QuantityAvailable.ShouldBe(10m));
    }

    [Fact]
    public async Task A_Reservation_Crossing_The_Threshold_Publishes_A_LowStockReached_Notification()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);

        var balanceId = await FindBalanceIdAsync(warehouseId, productId);
        await InventoryBalanceAppService.SetLowStockThresholdAsync(
            balanceId,
            new SetLowStockThresholdDto { Threshold = 8m, ConcurrencyStamp = (await StampOfBalanceAsync(balanceId)).ConcurrencyStamp });

        using var subscription = _broadcaster.Subscribe();

        await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 3m, UnitPrice = 20m, WarehouseId = warehouseId } }
        });

        var notifications = await subscription.DrainAsync();

        var lowStock = notifications
            .Where(n => n.ProductId == productId && n.Type == StockNotificationType.LowStockReached)
            .ToList();
        lowStock.ShouldNotBeEmpty();

        var latest = lowStock.Last();
        latest.ShouldSatisfyAllConditions(
            () => latest.IsBelowThreshold.ShouldBeTrue(),
            () => latest.LowStockThreshold.ShouldBe(8m),
            () => latest.QuantityAvailable.ShouldBe(7m));
    }

    [Fact]
    public async Task A_Rejected_Oversell_Publishes_No_Notification()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 3m);

        using var subscription = _broadcaster.Subscribe();

        await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => SaleAppService.CreateAsync(new CreateSaleDto
            {
                Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 5m, UnitPrice = 20m, WarehouseId = warehouseId } }
            }));

        var notifications = await subscription.DrainAsync();

        notifications.ShouldNotContain(n => n.ProductId == productId);
    }
}
