using System;

namespace CUInventory.Warehousing.Events;

public record ShipmentReceivedDomainEvent(Guid ShipmentId, DateTime ReceivedAt);
