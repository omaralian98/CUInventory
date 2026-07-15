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
    public Quantity QuantityOnHand { get; private set; }
    public Quantity QuantityReserved { get; private set; }
    public decimal? LowStockThreshold { get; private set; }

    public decimal QuantityAvailable => QuantityOnHand.Value - QuantityReserved.Value;

    protected InventoryBalance()
    {
    }

    internal InventoryBalance(Guid id, Guid warehouseId, Guid productId, decimal? lowStockThreshold = null) : base(id)
    {
        WarehouseId = warehouseId;
        ProductId = productId;
        QuantityOnHand = Quantity.Zero;
        QuantityReserved = Quantity.Zero;
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

        QuantityOnHand = QuantityOnHand.Add(quantity);
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
        QuantityReserved = QuantityReserved.Add(quantity);
        RaiseStockChanged(now);
        CheckLowStock(availableBefore, now);
    }

    internal void ReleaseReservation(Quantity quantity, DateTime now)
    {
        GuardPositive(quantity);

        if (quantity.Value > QuantityReserved.Value)
        {
            throw new InsufficientReservedStockDomainException(Id, quantity.Value, QuantityReserved.Value);
        }

        QuantityReserved = QuantityReserved.Subtract(quantity);
        RaiseStockChanged(now);
    }

    internal void ConfirmReservation(Quantity quantity, DateTime now)
    {
        GuardPositive(quantity);

        if (quantity.Value > QuantityReserved.Value)
        {
            throw new InsufficientReservedStockDomainException(Id, quantity.Value, QuantityReserved.Value);
        }

        QuantityReserved = QuantityReserved.Subtract(quantity);
        QuantityOnHand = QuantityOnHand.Subtract(quantity);
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
        QuantityOnHand = QuantityOnHand.Subtract(quantity);
        RaiseStockChanged(now);
        CheckLowStock(availableBefore, now);
    }

    private void RaiseStockChanged(DateTime now)
    {
        AddLocalEvent(new StockChangedDomainEvent(Id, now));
    }

    private void CheckLowStock(decimal availableBefore, DateTime now)
    {
        if (!LowStockRule.IsBelowThreshold(LowStockThreshold, availableBefore) &&
            LowStockRule.IsBelowThreshold(LowStockThreshold, QuantityAvailable))
        {
            AddLocalEvent(new LowStockReachedDomainEvent(Id, now));
        }
    }

    private static void GuardPositive(Quantity quantity)
    {
        Guard.Positive(quantity, nameof(quantity));
    }
}
