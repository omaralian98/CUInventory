using System;

namespace CUInventory.Inventory.Events;

public record StockChangedDomainEvent(Guid InventoryBalanceId, DateTime ChangedAt);
