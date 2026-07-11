using System;
using System.Collections.Generic;
using CUInventory.Common;
using CUInventory.Inventory;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities;

namespace CUInventory.Sales.Entities;

public class SaleLine : Entity<Guid>
{
    private readonly List<SaleAllocation> _allocations = [];

    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public AllocationStrategyKind Kind { get; private set; }
    public Guid? WarehouseId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public Guid? LotId { get; private set; }
    public IReadOnlyCollection<SaleAllocation> Allocations => _allocations;

    protected SaleLine()
    {
    }

    internal SaleLine(
        Guid id,
        Guid saleId,
        Guid productId,
        Quantity quantity,
        Money unitPrice,
        AllocationStrategyKind kind,
        Guid? warehouseId,
        Guid? supplierId,
        Guid? lotId) : base(id)
    {
        SaleId = saleId;
        ProductId = productId;
        Quantity = Guard.NotNull(quantity, nameof(quantity));
        UnitPrice = Guard.NotNull(unitPrice, nameof(unitPrice));
        Kind = kind;
        WarehouseId = warehouseId;
        SupplierId = supplierId;
        LotId = lotId;
    }

    internal void AddReservation(Guid allocationId, Guid warehouseId, Quantity quantity)
    {
        _allocations.Add(new SaleAllocation(allocationId, Id, warehouseId, quantity));
    }

    internal void AddAllocation(Guid allocationId, Guid warehouseId, Guid inventoryLotId, Guid? supplierId, Money unitCost, Quantity quantity)
    {
        _allocations.Add(new SaleAllocation(allocationId, Id, warehouseId, inventoryLotId, supplierId, unitCost, quantity));
    }

    internal void ClearAllocations()
    {
        _allocations.Clear();
    }
}
