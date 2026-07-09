using System;

namespace CUInventory.Inventory.Events;

public record InventoryAdjustedDomainEvent(Guid InventoryAdjustmentId, DateTime AdjustedAt);
