namespace CUInventory;

public static class CUInventoryDomainErrorCodes
{
    /* You can add your business exception error codes here, as constants */

    public const string CategoryNameAlreadyExists = "CUInventory:Catalog:001";
    public const string ProductSkuAlreadyExists = "CUInventory:Catalog:002";

    public const string SupplierEmailAlreadyExists = "CUInventory:Procurement:001";
    public const string SupplierPhoneNumberAlreadyExists = "CUInventory:Procurement:002";
}
