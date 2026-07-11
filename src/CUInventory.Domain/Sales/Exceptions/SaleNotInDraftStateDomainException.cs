using System;
using Volo.Abp;

namespace CUInventory.Sales.Exceptions;

public class SaleNotInDraftStateDomainException : BusinessException
{
    public SaleNotInDraftStateDomainException(Guid saleId, SaleStatus status)
        : base(CUInventoryDomainErrorCodes.SaleNotInDraftState)
    {
        WithData("SaleId", saleId);
        WithData("Status", status);
    }
}
