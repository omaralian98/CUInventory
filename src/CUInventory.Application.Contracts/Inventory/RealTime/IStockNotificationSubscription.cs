using System;
using System.Collections.Generic;
using System.Threading;

namespace CUInventory.Inventory.RealTime;

public interface IStockNotificationSubscription : IDisposable
{
    IAsyncEnumerable<StockNotificationDto> ReadAllAsync(CancellationToken cancellationToken);
}