using System;
using CUInventory.Inventory.Aggregates;
using CUInventory.ValueObjects;

namespace CUInventory.Inventory;

public static class StockTransferTestFactory
{
    public static StockTransfer NewTransfer(Guid source, Guid destination, Guid product, decimal quantity = 4m)
        => new(
            Guid.NewGuid(),
            source,
            destination,
            [new StockTransferLineData(Guid.NewGuid(), product, new Quantity(quantity))]);
}
