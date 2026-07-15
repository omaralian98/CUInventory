using CUInventory.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace CUInventory.Permissions;

public class CUInventoryPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(CUInventoryPermissions.GroupName, L("Permission:CUInventory"));

        var categories = group.AddPermission(CUInventoryPermissions.Categories.Default, L("Permission:Categories"));
        categories.AddChild(CUInventoryPermissions.Categories.Create, L("Permission:Create"));
        categories.AddChild(CUInventoryPermissions.Categories.Edit, L("Permission:Edit"));
        categories.AddChild(CUInventoryPermissions.Categories.Delete, L("Permission:Delete"));

        var products = group.AddPermission(CUInventoryPermissions.Products.Default, L("Permission:Products"));
        products.AddChild(CUInventoryPermissions.Products.Create, L("Permission:Create"));
        products.AddChild(CUInventoryPermissions.Products.Edit, L("Permission:Edit"));
        products.AddChild(CUInventoryPermissions.Products.Delete, L("Permission:Delete"));

        var suppliers = group.AddPermission(CUInventoryPermissions.Suppliers.Default, L("Permission:Suppliers"));
        suppliers.AddChild(CUInventoryPermissions.Suppliers.Create, L("Permission:Create"));
        suppliers.AddChild(CUInventoryPermissions.Suppliers.Edit, L("Permission:Edit"));
        suppliers.AddChild(CUInventoryPermissions.Suppliers.Delete, L("Permission:Delete"));

        var warehouses = group.AddPermission(CUInventoryPermissions.Warehouses.Default, L("Permission:Warehouses"));
        warehouses.AddChild(CUInventoryPermissions.Warehouses.Create, L("Permission:Create"));
        warehouses.AddChild(CUInventoryPermissions.Warehouses.Edit, L("Permission:Edit"));
        warehouses.AddChild(CUInventoryPermissions.Warehouses.Delete, L("Permission:Delete"));

        var purchaseOrders = group.AddPermission(CUInventoryPermissions.PurchaseOrders.Default, L("Permission:PurchaseOrders"));
        purchaseOrders.AddChild(CUInventoryPermissions.PurchaseOrders.Create, L("Permission:Create"));
        purchaseOrders.AddChild(CUInventoryPermissions.PurchaseOrders.Edit, L("Permission:Edit"));
        purchaseOrders.AddChild(CUInventoryPermissions.PurchaseOrders.Delete, L("Permission:Delete"));
        purchaseOrders.AddChild(CUInventoryPermissions.PurchaseOrders.Confirm, L("Permission:Confirm"));
        purchaseOrders.AddChild(CUInventoryPermissions.PurchaseOrders.Cancel, L("Permission:Cancel"));

        var shipments = group.AddPermission(CUInventoryPermissions.Shipments.Default, L("Permission:Shipments"));
        shipments.AddChild(CUInventoryPermissions.Shipments.Create, L("Permission:Create"));
        shipments.AddChild(CUInventoryPermissions.Shipments.Edit, L("Permission:Edit"));
        shipments.AddChild(CUInventoryPermissions.Shipments.Delete, L("Permission:Delete"));
        shipments.AddChild(CUInventoryPermissions.Shipments.Dispatch, L("Permission:Dispatch"));
        shipments.AddChild(CUInventoryPermissions.Shipments.Receive, L("Permission:Receive"));

        var stockTransfers = group.AddPermission(CUInventoryPermissions.StockTransfers.Default, L("Permission:StockTransfers"));
        stockTransfers.AddChild(CUInventoryPermissions.StockTransfers.Create, L("Permission:Create"));
        stockTransfers.AddChild(CUInventoryPermissions.StockTransfers.Edit, L("Permission:Edit"));
        stockTransfers.AddChild(CUInventoryPermissions.StockTransfers.Delete, L("Permission:Delete"));
        stockTransfers.AddChild(CUInventoryPermissions.StockTransfers.Dispatch, L("Permission:Dispatch"));
        stockTransfers.AddChild(CUInventoryPermissions.StockTransfers.Receive, L("Permission:Receive"));
        stockTransfers.AddChild(CUInventoryPermissions.StockTransfers.Cancel, L("Permission:Cancel"));

        var sales = group.AddPermission(CUInventoryPermissions.Sales.Default, L("Permission:Sales"));
        sales.AddChild(CUInventoryPermissions.Sales.Create, L("Permission:Create"));
        sales.AddChild(CUInventoryPermissions.Sales.Edit, L("Permission:Edit"));
        sales.AddChild(CUInventoryPermissions.Sales.Delete, L("Permission:Delete"));
        sales.AddChild(CUInventoryPermissions.Sales.Confirm, L("Permission:Confirm"));
        sales.AddChild(CUInventoryPermissions.Sales.Cancel, L("Permission:Cancel"));

        var inventoryBalances = group.AddPermission(CUInventoryPermissions.InventoryBalances.Default, L("Permission:InventoryBalances"));
        inventoryBalances.AddChild(CUInventoryPermissions.InventoryBalances.SetThreshold, L("Permission:SetThreshold"));
        inventoryBalances.AddChild(CUInventoryPermissions.InventoryBalances.SubscribeNotifications, L("Permission:SubscribeNotifications"));

        group.AddPermission(CUInventoryPermissions.InventoryLots.Default, L("Permission:InventoryLots"));

        group.AddPermission(CUInventoryPermissions.Reports.Default, L("Permission:Reports"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CUInventoryResource>(name);
    }
}
