using System;
using System.Threading.Tasks;
using CUInventory.Inventory.Events;
using CUInventory.Inventory.RealTime;
using CUInventory.Inventory.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Volo.Abp.Uow;

namespace CUInventory.Inventory.RealTime;

public class StockNotificationEventHandler(
    IInventoryBalanceRepository balanceRepository,
    IStockNotificationBroadcaster broadcaster,
    IUnitOfWorkManager unitOfWorkManager)
    : ILocalEventHandler<StockChangedDomainEvent>, 
        ILocalEventHandler<LowStockReachedDomainEvent>, 
        ITransientDependency
{
    public Task HandleEventAsync(StockChangedDomainEvent eventData)
    {
        return BroadcastAsync(eventData.InventoryBalanceId, StockNotificationType.StockChanged, eventData.ChangedAt);
    }

    public Task HandleEventAsync(LowStockReachedDomainEvent eventData)
    {
        return BroadcastAsync(eventData.InventoryBalanceId, StockNotificationType.LowStockReached, eventData.ReachedAt);
    }

    private async Task BroadcastAsync(Guid balanceId, StockNotificationType type, DateTime occurredAt)
    {
        var balance = await balanceRepository.GetAsync(balanceId);

        var notification = new StockNotificationDto
        {
            Type = type,
            InventoryBalanceId = balance.Id,
            WarehouseId = balance.WarehouseId,
            ProductId = balance.ProductId,
            QuantityOnHand = balance.QuantityOnHand,
            QuantityReserved = balance.QuantityReserved,
            QuantityAvailable = balance.QuantityAvailable,
            LowStockThreshold = balance.LowStockThreshold,
            IsBelowThreshold = balance.LowStockThreshold is { } threshold && balance.QuantityAvailable < threshold,
            OccurredAt = occurredAt,
            TenantId = balance.TenantId
        };

        var unitOfWork = unitOfWorkManager.Current;
        if (unitOfWork == null)
        {
            broadcaster.Publish(notification);
            return;
        }

        unitOfWork.OnCompleted(() =>
        {
            broadcaster.Publish(notification);
            return Task.CompletedTask;
        });
    }
}
