using System;
using CUInventory.Common;
using CUInventory.Inventory.Events;
using CUInventory.Inventory.Exceptions;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CUInventory.Inventory.Aggregates;

public class InventoryBalance : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal QuantityOnHand { get; private set; }
    public decimal QuantityReserved { get; private set; }
    public decimal? LowStockThreshold { get; private set; }

    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;

    protected InventoryBalance()
    {
    }

    internal InventoryBalance(Guid id, Guid warehouseId, Guid productId, decimal? lowStockThreshold = null) : base(id)
    {
        WarehouseId = warehouseId;
        ProductId = productId;
        QuantityOnHand = 0m;
        QuantityReserved = 0m;
        SetLowStockThreshold(lowStockThreshold);
    }

    public void SetLowStockThreshold(decimal? threshold)
    {
        Guard.NonNegative(threshold, nameof(threshold));

        LowStockThreshold = threshold;
    }

    internal void Increase(Quantity quantity, DateTime now)
    {
        GuardPositive(quantity);

        QuantityOnHand += quantity.Value;
        RaiseStockChanged(now);
    }

    internal void Reserve(Quantity quantity, DateTime now)
    {
        GuardPositive(quantity);

        if (QuantityAvailable < quantity.Value)
        {
            throw new InsufficientStockDomainException(ProductId, WarehouseId, quantity.Value, QuantityAvailable);
        }

        var availableBefore = QuantityAvailable;
        QuantityReserved += quantity.Value;
        RaiseStockChanged(now);
        CheckLowStock(availableBefore, now);
    }

    internal void ReleaseReservation(Quantity quantity, DateTime now)
    {
        GuardPositive(quantity);

        if (quantity.Value > QuantityReserved)
        {
            throw new InsufficientReservedStockDomainException(Id, quantity.Value, QuantityReserved);
        }

        QuantityReserved -= quantity.Value;
        RaiseStockChanged(now);
    }

    internal void ConfirmReservation(Quantity quantity, DateTime now)
    {
        GuardPositive(quantity);

        if (quantity.Value > QuantityReserved)
        {
            throw new InsufficientReservedStockDomainException(Id, quantity.Value, QuantityReserved);
        }

        QuantityReserved -= quantity.Value;
        QuantityOnHand -= quantity.Value;
        RaiseStockChanged(now);
    }

    internal void DeductDirect(Quantity quantity, DateTime now)
    {
        GuardPositive(quantity);

        if (QuantityAvailable < quantity.Value)
        {
            throw new InsufficientStockDomainException(ProductId, WarehouseId, quantity.Value, QuantityAvailable);
        }

        var availableBefore = QuantityAvailable;
        QuantityOnHand -= quantity.Value;
        RaiseStockChanged(now);
        CheckLowStock(availableBefore, now);
    }

    private void RaiseStockChanged(DateTime now)
    {
        AddLocalEvent(new StockChangedDomainEvent(Id, now));
    }

    private void CheckLowStock(decimal availableBefore, DateTime now)
    {
        if (LowStockThreshold is { } threshold && availableBefore >= threshold && QuantityAvailable < threshold)
        {
            AddLocalEvent(new LowStockReachedDomainEvent(Id, now));
        }
    }

    private static void GuardPositive(Quantity quantity)
    {
        Guard.Positive(quantity, nameof(quantity));
    }
}
