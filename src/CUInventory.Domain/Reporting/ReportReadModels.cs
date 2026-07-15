using System;
using System.Collections.Generic;
using CUInventory.Inventory;

namespace CUInventory.Reporting;

public record ReportPage<T>(IReadOnlyList<T> Items, long TotalCount);

public record SalesBySourceReport(
    IReadOnlyList<SalesBySourceRow> Items,
    long TotalCount,
    decimal TotalQuantitySold,
    decimal TotalRevenue,
    decimal TotalCost);

public record RemainingStockReport(
    IReadOnlyList<RemainingStockRow> Items,
    long TotalCount,
    decimal TotalRemainingQuantity,
    decimal TotalValueAtCost);

public record InventoryValuationReport(
    IReadOnlyList<InventoryValuationRow> Items,
    long TotalCount,
    decimal GrandTotalQuantity,
    decimal GrandTotalValue);

public record SalesBySourceRow(
    Guid? SupplierId,
    Guid ProductId,
    decimal QuantitySold,
    decimal Revenue,
    decimal Cost);

public record SalesBySourceDetailRow(
    Guid SaleId,
    Guid SaleLineId,
    Guid ProductId,
    Guid? SupplierId,
    Guid? InventoryLotId,
    Guid WarehouseId,
    decimal Quantity,
    decimal UnitPrice,
    decimal UnitCost,
    DateTime? ConfirmedAt);

public record RemainingStockRow(
    Guid WarehouseId,
    Guid? SupplierId,
    Guid ProductId,
    decimal RemainingQuantity,
    decimal ValueAtCost);

public record RemainingStockDetailRow(
    Guid LotId,
    Guid ProductId,
    Guid WarehouseId,
    Guid? SupplierId,
    Guid? ShipmentLineId,
    InventoryLotSource Source,
    decimal RemainingQuantity,
    decimal UnitCost,
    decimal ValueAtCost,
    DateTime ReceivedAt);

public record InventoryValuationRow(
    Guid WarehouseId,
    Guid? CategoryId,
    decimal TotalQuantity,
    decimal TotalValue);

public record LowStockRow(
    Guid WarehouseId,
    Guid ProductId,
    decimal QuantityOnHand,
    decimal QuantityReserved,
    decimal QuantityAvailable,
    decimal? LowStockThreshold);
