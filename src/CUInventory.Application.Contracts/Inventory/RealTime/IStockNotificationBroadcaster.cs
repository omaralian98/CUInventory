namespace CUInventory.Inventory.RealTime;

public interface IStockNotificationBroadcaster
{
    void Publish(StockNotificationDto notification);

    IStockNotificationSubscription Subscribe();
}