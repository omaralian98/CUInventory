using System;
using System.Collections.Generic;
using CUInventory.Common;
using CUInventory.Inventory;
using CUInventory.Sales.Entities;
using CUInventory.Sales.Events;
using CUInventory.Sales.Exceptions;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CUInventory.Sales.Aggregates;

public class Sale : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    private readonly List<SaleLine> _lines = [];

    public Guid? TenantId { get; protected set; }
    public SaleStatus Status { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public int LinesCount { get; private set; }
    public Money TotalAmount { get; private set; }
    public IReadOnlyCollection<SaleLine> Lines => _lines;

    protected Sale()
    {
    }

    public Sale(Guid id, IReadOnlyCollection<SaleLineData> lines) : base(id)
    {
        Status = SaleStatus.Draft;

        Guard.NotNull(lines, nameof(lines));
        var total = Money.Zero;
        foreach (var line in lines)
        {
            _lines.Add(new SaleLine(line.Id, Id, line.ProductId, line.Quantity, line.UnitPrice, line.Kind, line.WarehouseId, line.SupplierId, line.LotId));
            total = total.Add(line.UnitPrice.Multiply(line.Quantity.Value));
        }

        LinesCount = _lines.Count;
        TotalAmount = total;
    }

    internal void Confirm(DateTime now)
    {
        if (Status != SaleStatus.Draft)
        {
            throw new SaleNotInDraftStateDomainException(Id, Status);
        }

        if (_lines.Count == 0)
        {
            throw new SaleHasNoLinesDomainException(Id);
        }

        Status = SaleStatus.Confirmed;
        ConfirmedAt = now;
        AddLocalEvent(new SaleCompletedDomainEvent(Id, now));
    }

    internal void Cancel()
    {
        if (Status != SaleStatus.Draft)
        {
            throw new SaleNotInDraftStateDomainException(Id, Status);
        }

        Status = SaleStatus.Cancelled;
    }
}
