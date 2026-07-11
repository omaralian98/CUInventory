using System;
using CUInventory.ValueObjects;

namespace CUInventory.Procurement;

public record PurchaseOrderLineData(
    Guid Id,
    Guid ProductId,
    Quantity OrderedQuantity,
    Money UnitCost);
