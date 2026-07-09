using System;

namespace CUInventory.Warehousing.Events;

public record ShipmentDispatchedDomainEvent(Guid ShipmentId, DateTime DispatchedAt);
