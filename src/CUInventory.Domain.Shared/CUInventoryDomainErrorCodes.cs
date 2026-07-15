namespace CUInventory;

public static class CUInventoryDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */

    public const string RequiredArgument = "CUInventory:Common:001";
    public const string ArgumentMustBePositive = "CUInventory:Common:002";
    public const string ArgumentMustBeNonNegative = "CUInventory:Common:003";
    public const string ArgumentInvalidFormat = "CUInventory:Common:004";

    public const string CategoryNameAlreadyExists = "CUInventory:Catalog:001";
    public const string ProductSkuAlreadyExists = "CUInventory:Catalog:002";

    public const string SupplierEmailAlreadyExists = "CUInventory:Procurement:001";
    public const string SupplierPhoneNumberAlreadyExists = "CUInventory:Procurement:002";
    public const string PurchaseOrderNotInDraftState = "CUInventory:Procurement:003";
    public const string PurchaseOrderHasNoLines = "CUInventory:Procurement:004";
    public const string PurchaseOrderNotConfirmed = "CUInventory:Procurement:005";
    public const string PurchaseOrderLineNotFound = "CUInventory:Procurement:006";
    public const string PurchaseOrderReceiptExceedsOrdered = "CUInventory:Procurement:007";
    public const string PurchaseOrderCannotBeCancelled = "CUInventory:Procurement:008";
    public const string PurchaseOrderDuplicateProductLine = "CUInventory:Procurement:009";

    public const string WarehouseCodeAlreadyExists = "CUInventory:Warehousing:001";
    public const string ShipmentNotInDraftState = "CUInventory:Warehousing:002";
    public const string ShipmentHasNoLines = "CUInventory:Warehousing:003";
    public const string ShipmentNotDispatched = "CUInventory:Warehousing:004";
    public const string ShipmentSupplierMismatch = "CUInventory:Warehousing:005";
    public const string ShipmentWarehouseMismatch = "CUInventory:Warehousing:006";
    public const string ShipmentExceedsOutstanding = "CUInventory:Warehousing:007";

    public const string InsufficientStock = "CUInventory:Inventory:001";
    public const string InsufficientReservedStock = "CUInventory:Inventory:002";
    public const string InventoryLotInsufficientRemaining = "CUInventory:Inventory:003";
    public const string MissingInventoryBalance = "CUInventory:Inventory:004";
    public const string StockTransferNotInDraftState = "CUInventory:Inventory:005";
    public const string StockTransferHasNoLines = "CUInventory:Inventory:006";
    public const string StockTransferNotDispatched = "CUInventory:Inventory:007";
    public const string StockTransferCannotBeCancelled = "CUInventory:Inventory:008";
    public const string StockTransferSameWarehouse = "CUInventory:Inventory:009";
    public const string InventoryLotRestoreExceedsOriginal = "CUInventory:Inventory:010";
    public const string InventoryLotInsufficientAvailable = "CUInventory:Inventory:011";
    public const string InventoryLotInsufficientReserved = "CUInventory:Inventory:012";
    public const string StockTransferCannotBeDeleted = "CUInventory:Inventory:013";
    public const string StockTransferDuplicateProductLine = "CUInventory:Inventory:014";

    public const string SaleNotInDraftState = "CUInventory:Sales:001";
    public const string SaleHasNoLines = "CUInventory:Sales:002";
    public const string SaleCannotBeDeleted = "CUInventory:Sales:003";
    public const string SaleAllocationHasNoLot = "CUInventory:Sales:004";
}
