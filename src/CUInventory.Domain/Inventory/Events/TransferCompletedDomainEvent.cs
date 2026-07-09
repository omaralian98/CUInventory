using System;

namespace CUInventory.Inventory.Events;

public record TransferCompletedDomainEvent(Guid StockTransferId, DateTime CompletedAt);
