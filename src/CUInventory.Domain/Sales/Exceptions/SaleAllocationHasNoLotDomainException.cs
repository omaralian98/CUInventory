using System;
using Volo.Abp;

namespace CUInventory.Sales.Exceptions;

public class SaleAllocationHasNoLotDomainException : BusinessException
{
    public SaleAllocationHasNoLotDomainException(Guid saleId, Guid saleAllocationId)
        : base(CUInventoryDomainErrorCodes.SaleAllocationHasNoLot)
    {
        WithData("SaleId", saleId);
        WithData("SaleAllocationId", saleAllocationId);
    }
}
