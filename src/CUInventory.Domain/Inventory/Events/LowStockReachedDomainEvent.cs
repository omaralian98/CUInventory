using System;

namespace CUInventory.Inventory.Events;

public record LowStockReachedDomainEvent(Guid InventoryBalanceId, DateTime ReachedAt);
