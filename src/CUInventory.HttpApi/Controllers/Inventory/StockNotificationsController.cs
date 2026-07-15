using System;
using System.Collections.Generic;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CUInventory.Controllers;
using CUInventory.Inventory.RealTime;
using CUInventory.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace CUInventory.Controllers.Inventory;

[Authorize(CUInventoryPermissions.InventoryBalances.SubscribeNotifications)]
[Route("api/inventory/stock-notifications")]
public class StockNotificationsController(
    IStockNotificationBroadcaster broadcaster,
    ICurrentTenant currentTenant)
    : CUInventoryController
{
    [HttpGet("stream")]
    [UnitOfWork(IsDisabled = true)]
    public async Task StreamAsync(CancellationToken cancellationToken)
    {
        var tenantId = currentTenant.Id;

        using var subscription = broadcaster.Subscribe();

        var result = TypedResults.ServerSentEvents(
            StreamForTenantAsync(subscription, tenantId, cancellationToken));

        await result.ExecuteAsync(HttpContext);
    }

    private static async IAsyncEnumerable<SseItem<StockNotificationDto?>> StreamForTenantAsync(
        IStockNotificationSubscription subscription,
        Guid? tenantId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return new SseItem<StockNotificationDto?>(null, "Connected");

        await foreach (var notification in subscription.ReadAllAsync(cancellationToken))
        {
            if (notification.TenantId != tenantId)
            {
                continue;
            }

            yield return new SseItem<StockNotificationDto?>(notification, notification.Type.ToString());
        }
    }
}
