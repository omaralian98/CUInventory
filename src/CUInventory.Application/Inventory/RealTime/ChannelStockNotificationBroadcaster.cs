using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace CUInventory.Inventory.RealTime;

public class ChannelStockNotificationBroadcaster : IStockNotificationBroadcaster, ISingletonDependency
{
    private const int SubscriberBufferSize = 100;

    private readonly ConcurrentDictionary<Guid, SubscriberChannels> _subscribers = new();

    public void Publish(StockNotificationDto notification)
    {
        foreach (var channels in _subscribers.Values)
        {
            var channel = notification.Type == StockNotificationType.LowStockReached
                ? channels.Alerts
                : channels.Stock;
            channel.Writer.TryWrite(notification);
        }
    }

    public IStockNotificationSubscription Subscribe()
    {
        var alerts = CreateSubscriberChannel();
        var stock = CreateSubscriberChannel();

        var id = Guid.NewGuid();
        _subscribers[id] = new SubscriberChannels(alerts, stock);

        return new Subscription(alerts.Reader, stock.Reader, () =>
        {
            _subscribers.TryRemove(id, out _);
            alerts.Writer.TryComplete();
            stock.Writer.TryComplete();
        });
    }

    private static Channel<StockNotificationDto> CreateSubscriberChannel()
    {
        return Channel.CreateBounded<StockNotificationDto>(
            new BoundedChannelOptions(SubscriberBufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest
            });
    }

    private sealed record SubscriberChannels(
        Channel<StockNotificationDto> Alerts,
        Channel<StockNotificationDto> Stock);

    private sealed class Subscription(
        ChannelReader<StockNotificationDto> alerts,
        ChannelReader<StockNotificationDto> stock,
        Action dispose)
        : IStockNotificationSubscription
    {
        public async IAsyncEnumerable<StockNotificationDto> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Task<bool>? alertsWait = null;
            Task<bool>? stockWait = null;
            var alertsOpen = true;
            var stockOpen = true;

            while (alertsOpen || stockOpen)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (alertsOpen && alerts.TryRead(out var alert))
                {
                    alertsWait = null;
                    yield return alert;
                    continue;
                }

                if (stockOpen && stock.TryRead(out var notification))
                {
                    stockWait = null;
                    yield return notification;
                    continue;
                }

                if (alertsOpen)
                {
                    alertsWait ??= alerts.WaitToReadAsync(cancellationToken).AsTask();
                }

                if (stockOpen)
                {
                    stockWait ??= stock.WaitToReadAsync(cancellationToken).AsTask();
                }

                if (alertsWait is not null && stockWait is not null)
                {
                    await Task.WhenAny(alertsWait, stockWait);
                }
                else
                {
                    await (alertsWait ?? stockWait)!;
                }

                if (alertsWait is { IsCompleted: true })
                {
                    alertsOpen = await alertsWait;
                    alertsWait = null;
                }

                if (stockWait is { IsCompleted: true })
                {
                    stockOpen = await stockWait;
                    stockWait = null;
                }
            }
        }

        public void Dispose()
        {
            dispose();
        }
    }
}
