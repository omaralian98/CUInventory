using System;
using Volo.Abp;

namespace CUInventory.Sales.Exceptions;

public class SaleHasNoLinesDomainException : BusinessException
{
    public SaleHasNoLinesDomainException(Guid saleId)
        : base(CUInventoryDomainErrorCodes.SaleHasNoLines)
    {
        WithData("SaleId", saleId);
    }
}
