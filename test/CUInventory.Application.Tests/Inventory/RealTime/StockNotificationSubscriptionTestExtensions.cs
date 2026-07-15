using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CUInventory.Inventory.RealTime;

public static class StockNotificationSubscriptionTestExtensions
{
    private static readonly TimeSpan HangGuard = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Disposes the subscription (completing its channel writers) and returns everything it had
    /// buffered, so draining finishes promptly instead of waiting out a timeout.
    /// </summary>
    public static async Task<List<StockNotificationDto>> DrainAsync(this IStockNotificationSubscription subscription)
    {
        subscription.Dispose();

        var items = new List<StockNotificationDto>();
        using var cts = new CancellationTokenSource(HangGuard);

        await foreach (var item in subscription.ReadAllAsync(cts.Token))
        {
            items.Add(item);
        }

        return items;
    }
}
