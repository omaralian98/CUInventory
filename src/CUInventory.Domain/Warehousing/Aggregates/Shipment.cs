using System;
using System.Collections.Generic;
using CUInventory.Common;
using CUInventory.ValueObjects;
using CUInventory.Warehousing.Entities;
using CUInventory.Warehousing.Events;
using CUInventory.Warehousing.Exceptions;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CUInventory.Warehousing.Aggregates;

public class Shipment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public DateTime? DispatchedAt { get; private set; }
    public DateTime? ReceivedAt { get; private set; }
    public IReadOnlyCollection<ShipmentLine> Lines => _lines;
    private readonly List<ShipmentLine> _lines = [];
    
    public Guid? TenantId { get; protected set; }

    protected Shipment()
    {
    }

    public Shipment(Guid id, Guid purchaseOrderId, Guid supplierId, Guid destinationWarehouseId, IReadOnlyCollection<ShipmentLineData> lines) : base(id)
    {
        PurchaseOrderId = purchaseOrderId;
        SupplierId = supplierId;
        DestinationWarehouseId = destinationWarehouseId;
        Status = ShipmentStatus.Draft;

        Guard.NotNull(lines, nameof(lines));
        foreach (var line in lines)
        {
            _lines.Add(new ShipmentLine(line.Id, Id, line.ProductId, line.Quantity, line.UnitCost));
        }
    }

    public void Dispatch(DateTime now)
    {
        if (Status != ShipmentStatus.Draft)
        {
            throw new ShipmentNotInDraftStateDomainException(Id, Status);
        }

        if (_lines.Count == 0)
        {
            throw new ShipmentHasNoLinesDomainException(Id);
        }

        Status = ShipmentStatus.Dispatched;
        DispatchedAt = now;
        AddLocalEvent(new ShipmentDispatchedDomainEvent(Id, now));
    }

    internal void MarkReceived(DateTime now)
    {
        if (Status != ShipmentStatus.Dispatched)
        {
            throw new ShipmentNotDispatchedDomainException(Id, Status);
        }

        Status = ShipmentStatus.Received;
        ReceivedAt = now;
        AddLocalEvent(new ShipmentReceivedDomainEvent(Id, now));
    }
}
