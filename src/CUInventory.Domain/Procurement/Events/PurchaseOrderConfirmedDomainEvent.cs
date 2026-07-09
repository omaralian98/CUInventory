using System;

namespace CUInventory.Procurement.Events;

public record PurchaseOrderConfirmedDomainEvent(Guid PurchaseOrderId, DateTime ConfirmedAt);
