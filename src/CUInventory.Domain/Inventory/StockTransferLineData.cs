using System;
using CUInventory.ValueObjects;

namespace CUInventory.Inventory;

public record StockTransferLineData(
    Guid Id,
    Guid ProductId,
    Quantity Quantity);
