using System;
using Volo.Abp;

namespace CUInventory.Warehousing.Exceptions;

public class ShipmentNotDispatchedDomainException : BusinessException
{
    public ShipmentNotDispatchedDomainException(Guid shipmentId, ShipmentStatus status)
        : base(CUInventoryDomainErrorCodes.ShipmentNotDispatched)
    {
        WithData("ShipmentId", shipmentId);
        WithData("Status", status);
    }
}
