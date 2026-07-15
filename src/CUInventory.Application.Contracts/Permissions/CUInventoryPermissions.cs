namespace CUInventory.Permissions;

public static class CUInventoryPermissions
{
    public const string GroupName = "CUInventory";

    public static class Categories
    {
        public const string Default = GroupName + "." + nameof(Categories);
        public const string Create = Default + "." + nameof(Create);
        public const string Edit = Default + "." + nameof(Edit);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public static class Products
    {
        public const string Default = GroupName + "." + nameof(Products);
        public const string Create = Default + "." + nameof(Create);
        public const string Edit = Default + "." + nameof(Edit);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public static class Suppliers
    {
        public const string Default = GroupName + "." + nameof(Suppliers);
        public const string Create = Default + "." + nameof(Create);
        public const string Edit = Default + "." + nameof(Edit);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public static class Warehouses
    {
        public const string Default = GroupName + "." + nameof(Warehouses);
        public const string Create = Default + "." + nameof(Create);
        public const string Edit = Default + "." + nameof(Edit);
        public const string Delete = Default + "." + nameof(Delete);
    }

    public static class PurchaseOrders
    {
        public const string Default = GroupName + "." + nameof(PurchaseOrders);
        public const string Create = Default + "." + nameof(Create);
        public const string Edit = Default + "." + nameof(Edit);
        public const string Delete = Default + "." + nameof(Delete);
        public const string Confirm = Default + "." + nameof(Confirm);
        public const string Cancel = Default + "." + nameof(Cancel);
    }

    public static class Shipments
    {
        public const string Default = GroupName + "." + nameof(Shipments);
        public const string Create = Default + "." + nameof(Create);
        public const string Edit = Default + "." + nameof(Edit);
        public const string Delete = Default + "." + nameof(Delete);
        public const string Dispatch = Default + "." + nameof(Dispatch);
        public const string Receive = Default + "." + nameof(Receive);
    }

    public static class StockTransfers
    {
        public const string Default = GroupName + "." + nameof(StockTransfers);
        public const string Create = Default + "." + nameof(Create);
        public const string Edit = Default + "." + nameof(Edit);
        public const string Delete = Default + "." + nameof(Delete);
        public const string Dispatch = Default + "." + nameof(Dispatch);
        public const string Receive = Default + "." + nameof(Receive);
        public const string Cancel = Default + "." + nameof(Cancel);
    }

    public static class Sales
    {
        public const string Default = GroupName + "." + nameof(Sales);
        public const string Create = Default + "." + nameof(Create);
        public const string Edit = Default + "." + nameof(Edit);
        public const string Delete = Default + "." + nameof(Delete);
        public const string Confirm = Default + "." + nameof(Confirm);
        public const string Cancel = Default + "." + nameof(Cancel);
    }

    public static class InventoryBalances
    {
        public const string Default = GroupName + "." + nameof(InventoryBalances);
        public const string SetThreshold = Default + "." + nameof(SetThreshold);
        public const string SubscribeNotifications = Default + "." + nameof(SubscribeNotifications);
    }

    public static class InventoryLots
    {
        public const string Default = GroupName + "." + nameof(InventoryLots);
    }

    public static class Reports
    {
        public const string Default = GroupName + "." + nameof(Reports);
    }
}
