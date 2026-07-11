using System;
using Volo.Abp;

namespace CUInventory.Warehousing.Exceptions;

public class ShipmentNotInDraftStateDomainException : BusinessException
{
    public ShipmentNotInDraftStateDomainException(Guid shipmentId, ShipmentStatus status)
        : base(CUInventoryDomainErrorCodes.ShipmentNotInDraftState)
    {
        WithData("ShipmentId", shipmentId);
        WithData("Status", status);
    }
}
