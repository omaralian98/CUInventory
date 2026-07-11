using System;
using CUInventory.ValueObjects;

namespace CUInventory.Warehousing;

public record ShipmentLineData(
    Guid Id,
    Guid ProductId,
    Quantity Quantity,
    Money UnitCost);
