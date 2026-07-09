using CUInventory.Catalog.ValueObjects;
using Volo.Abp;
using CUInventory.ValueObjects;

namespace CUInventory.Catalog.Exceptions;

public class ProductSkuAlreadyExistsDomainException : BusinessException
{
    public ProductSkuAlreadyExistsDomainException(Sku sku)
        : base(CUInventoryDomainErrorCodes.ProductSkuAlreadyExists)
    {
        WithData("Sku", sku.Value);
    }
}
