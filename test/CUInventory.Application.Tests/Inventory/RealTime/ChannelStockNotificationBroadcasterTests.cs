using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.RealTime;
using Shouldly;
using Xunit;

namespace CUInventory.Inventory.RealTime;

public class ChannelStockNotificationBroadcasterTests
{
    private readonly ChannelStockNotificationBroadcaster _broadcaster = new();

    [Fact]
    public async Task Published_Notification_Reaches_A_Subscriber()
    {
        using var subscription = _broadcaster.Subscribe();

        _broadcaster.Publish(NotificationFor(1m));

        var received = await subscription.DrainAsync();

        received.ShouldHaveSingleItem().QuantityOnHand.ShouldBe(1m);
    }

    [Fact]
    public async Task Every_Subscriber_Receives_A_Published_Notification()
    {
        using var first = _broadcaster.Subscribe();
        using var second = _broadcaster.Subscribe();

        _broadcaster.Publish(NotificationFor(7m));

        var toFirst = await first.DrainAsync();
        var toSecond = await second.DrainAsync();

        toFirst.ShouldHaveSingleItem().QuantityOnHand.ShouldBe(7m);
        toSecond.ShouldHaveSingleItem().QuantityOnHand.ShouldBe(7m);
    }

    [Fact]
    public async Task A_Disposed_Subscription_Receives_Nothing_Further()
    {
        var subscription = _broadcaster.Subscribe();
        subscription.Dispose();

        _broadcaster.Publish(NotificationFor(3m));

        var received = await subscription.DrainAsync();

        received.ShouldBeEmpty();
    }

    [Fact]
    public async Task An_Overflowing_Subscriber_Keeps_Only_The_Newest_Notifications()
    {
        using var subscription = _broadcaster.Subscribe();

        for (var i = 1; i <= 150; i++)
        {
            _broadcaster.Publish(NotificationFor(i));
        }

        var received = await subscription.DrainAsync();

        received.Count.ShouldBe(100);
        received[^1].QuantityOnHand.ShouldBe(150m);
    }

    [Fact]
    public async Task A_LowStock_Alert_Survives_A_Notification_Flood_To_A_Slow_Subscriber()
    {
        using var subscription = _broadcaster.Subscribe();

        for (var i = 1; i <= 100; i++)
        {
            _broadcaster.Publish(NotificationFor(i));
        }

        _broadcaster.Publish(NotificationFor(42m, StockNotificationType.LowStockReached));

        for (var i = 101; i <= 200; i++)
        {
            _broadcaster.Publish(NotificationFor(i));
        }

        var received = await subscription.DrainAsync();

        received.ShouldSatisfyAllConditions(
            () => received.Single(n => n.Type == StockNotificationType.LowStockReached).QuantityOnHand.ShouldBe(42m),
            () => received.Last(n => n.Type == StockNotificationType.StockChanged).QuantityOnHand.ShouldBe(200m));
    }

    private static StockNotificationDto NotificationFor(
        decimal onHand,
        StockNotificationType type = StockNotificationType.StockChanged)
    {
        return new StockNotificationDto
        {
            Type = type,
            InventoryBalanceId = Guid.NewGuid(),
            QuantityOnHand = onHand,
            OccurredAt = DateTime.UnixEpoch
        };
    }
}
