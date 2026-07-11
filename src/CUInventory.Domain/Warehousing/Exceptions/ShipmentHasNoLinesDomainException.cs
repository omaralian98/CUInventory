using System;
using Volo.Abp;

namespace CUInventory.Warehousing.Exceptions;

public class ShipmentHasNoLinesDomainException : BusinessException
{
    public ShipmentHasNoLinesDomainException(Guid shipmentId)
        : base(CUInventoryDomainErrorCodes.ShipmentHasNoLines)
    {
        WithData("ShipmentId", shipmentId);
    }
}
