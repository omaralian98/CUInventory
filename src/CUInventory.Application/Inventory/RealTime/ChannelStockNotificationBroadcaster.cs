using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using Volo.Abp.DependencyInjection;

namespace CUInventory.Inventory.RealTime;

public class ChannelStockNotificationBroadcaster : IStockNotificationBroadcaster, ISingletonDependency
{
    private const int SubscriberBufferSize = 100;

    private readonly ConcurrentDictionary<Guid, Channel<StockNotificationDto>> _subscribers = new();

    public void Publish(StockNotificationDto notification)
    {
        foreach (var channel in _subscribers.Values)
        {
            channel.Writer.TryWrite(notification);
        }
    }

    public IStockNotificationSubscription Subscribe()
    {
        var channel = Channel.CreateBounded<StockNotificationDto>(
            new BoundedChannelOptions(SubscriberBufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                SingleWriter = false
            });

        var id = Guid.NewGuid();
        _subscribers[id] = channel;

        return new Subscription(channel.Reader, () =>
        {
            _subscribers.TryRemove(id, out _);
            channel.Writer.TryComplete();
        });
    }

    private sealed class Subscription(ChannelReader<StockNotificationDto> reader, Action dispose)
        : IStockNotificationSubscription
    {
        public IAsyncEnumerable<StockNotificationDto> ReadAllAsync(CancellationToken cancellationToken)
        {
            return reader.ReadAllAsync(cancellationToken);
        }

        public void Dispose()
        {
            dispose();
        }
    }
}
