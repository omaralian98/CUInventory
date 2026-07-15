using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CUInventory.Inventory.RealTime;
using Shouldly;
using Xunit;

namespace CUInventory.Inventory.RealTime;

public class ChannelStockNotificationBroadcasterTests
{
    private static readonly TimeSpan DrainTimeout = TimeSpan.FromSeconds(1);

    private readonly ChannelStockNotificationBroadcaster _broadcaster = new();

    [Fact]
    public async Task Published_Notification_Reaches_A_Subscriber()
    {
        using var subscription = _broadcaster.Subscribe();

        _broadcaster.Publish(NotificationFor(1m));

        var received = await DrainAsync(subscription);

        received.ShouldHaveSingleItem().QuantityOnHand.ShouldBe(1m);
    }

    [Fact]
    public async Task Every_Subscriber_Receives_A_Published_Notification()
    {
        using var first = _broadcaster.Subscribe();
        using var second = _broadcaster.Subscribe();

        _broadcaster.Publish(NotificationFor(7m));

        var toFirst = await DrainAsync(first);
        var toSecond = await DrainAsync(second);

        toFirst.ShouldHaveSingleItem().QuantityOnHand.ShouldBe(7m);
        toSecond.ShouldHaveSingleItem().QuantityOnHand.ShouldBe(7m);
    }

    [Fact]
    public async Task A_Disposed_Subscription_Receives_Nothing_Further()
    {
        var subscription = _broadcaster.Subscribe();
        subscription.Dispose();

        _broadcaster.Publish(NotificationFor(3m));

        var received = await DrainAsync(subscription);

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

        var received = await DrainAsync(subscription);

        received.Count.ShouldBe(100);
        received[^1].QuantityOnHand.ShouldBe(150m);
    }

    private static StockNotificationDto NotificationFor(decimal onHand)
    {
        return new StockNotificationDto
        {
            Type = StockNotificationType.StockChanged,
            InventoryBalanceId = Guid.NewGuid(),
            QuantityOnHand = onHand,
            OccurredAt = DateTime.UnixEpoch
        };
    }

    private static async Task<List<StockNotificationDto>> DrainAsync(IStockNotificationSubscription subscription)
    {
        var items = new List<StockNotificationDto>();
        using var cts = new CancellationTokenSource(DrainTimeout);

        try
        {
            await foreach (var item in subscription.ReadAllAsync(cts.Token))
            {
                items.Add(item);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return items;
    }
}
