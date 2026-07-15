using System;
using System.Threading.Tasks;
using CUInventory.Inventory.Dtos;
using CUInventory.Inventory.Events;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Xunit;

namespace CUInventory.Inventory.RealTime;

public abstract class StockNotificationEventHandlerTests<TStartupModule> : CUInventoryStockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IStockNotificationBroadcaster _broadcaster;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly StockNotificationEventHandler _handler;

    protected StockNotificationEventHandlerTests()
    {
        _broadcaster = GetRequiredService<IStockNotificationBroadcaster>();
        _unitOfWorkManager = GetRequiredService<IUnitOfWorkManager>();
        _handler = GetRequiredService<StockNotificationEventHandler>();
    }

    [Fact]
    public async Task HandleEventAsync_StockChanged_Defers_Publish_Until_The_UnitOfWork_Completes()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);
        var balanceId = await FindBalanceIdAsync(warehouseId, productId);
        var occurredAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        using var probe = _broadcaster.Subscribe();
        using var subscription = _broadcaster.Subscribe();

        using (var unitOfWork = _unitOfWorkManager.Begin(new AbpUnitOfWorkOptions(), requiresNew: true))
        {
            await _handler.HandleEventAsync(new StockChangedDomainEvent(balanceId, occurredAt));

            var beforeCompletion = await probe.DrainAsync();
            beforeCompletion.ShouldBeEmpty();

            await unitOfWork.CompleteAsync();
        }

        var notifications = await subscription.DrainAsync();
        var notification = notifications.ShouldHaveSingleItem();
        notification.ShouldSatisfyAllConditions(
            () => notification.Type.ShouldBe(StockNotificationType.StockChanged),
            () => notification.InventoryBalanceId.ShouldBe(balanceId),
            () => notification.WarehouseId.ShouldBe(warehouseId),
            () => notification.ProductId.ShouldBe(productId),
            () => notification.QuantityOnHand.ShouldBe(10m),
            () => notification.QuantityReserved.ShouldBe(0m),
            () => notification.QuantityAvailable.ShouldBe(10m),
            () => notification.IsBelowThreshold.ShouldBeFalse(),
            () => notification.OccurredAt.ShouldBe(occurredAt));
    }

    [Fact]
    public async Task HandleEventAsync_LowStockReached_Maps_The_Threshold_Flag()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 5m);
        var balanceId = await FindBalanceIdAsync(warehouseId, productId);
        await InventoryBalanceAppService.SetLowStockThresholdAsync(
            balanceId,
            new SetLowStockThresholdDto { Threshold = 8m, ConcurrencyStamp = (await StampOfBalanceAsync(balanceId)).ConcurrencyStamp });
        var occurredAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        using var subscription = _broadcaster.Subscribe();

        using (var unitOfWork = _unitOfWorkManager.Begin(new AbpUnitOfWorkOptions(), requiresNew: true))
        {
            await _handler.HandleEventAsync(new LowStockReachedDomainEvent(balanceId, occurredAt));
            await unitOfWork.CompleteAsync();
        }

        var notifications = await subscription.DrainAsync();
        var notification = notifications.ShouldHaveSingleItem();
        notification.ShouldSatisfyAllConditions(
            () => notification.Type.ShouldBe(StockNotificationType.LowStockReached),
            () => notification.InventoryBalanceId.ShouldBe(balanceId),
            () => notification.LowStockThreshold.ShouldBe(8m),
            () => notification.QuantityAvailable.ShouldBe(5m),
            () => notification.IsBelowThreshold.ShouldBeTrue(),
            () => notification.OccurredAt.ShouldBe(occurredAt));
    }
}
