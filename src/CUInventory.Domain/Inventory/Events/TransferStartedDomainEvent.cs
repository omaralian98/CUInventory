using System;

namespace CUInventory.Inventory.Events;

public record TransferStartedDomainEvent(Guid StockTransferId, DateTime StartedAt);
