using System;

namespace CUInventory.Sales.Events;

public record SaleCompletedDomainEvent(Guid SaleId, DateTime CompletedAt);
