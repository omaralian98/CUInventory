using System;
using Volo.Abp;

namespace CUInventory.Sales.Exceptions;

public class SaleCannotBeDeletedDomainException : BusinessException
{
    public SaleCannotBeDeletedDomainException(Guid saleId, SaleStatus status)
        : base(CUInventoryDomainErrorCodes.SaleCannotBeDeleted)
    {
        WithData("SaleId", saleId);
        WithData("Status", status);
    }
}
