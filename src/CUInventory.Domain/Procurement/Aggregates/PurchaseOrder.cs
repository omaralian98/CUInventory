using System;
using System.Collections.Generic;
using System.Linq;
using CUInventory.Common;
using CUInventory.Procurement.Entities;
using CUInventory.Procurement.Events;
using CUInventory.Procurement.Exceptions;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CUInventory.Procurement.Aggregates;

public class PurchaseOrder : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid SupplierId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public int LinesCount { get; private set; }

    public Guid? TenantId { get; protected set; }
    
    private readonly List<PurchaseOrderLine> _lines = [];
    public IReadOnlyCollection<PurchaseOrderLine> Lines => _lines;

    protected PurchaseOrder()
    {
    }

    public PurchaseOrder(Guid id, Guid supplierId, Guid destinationWarehouseId, IReadOnlyCollection<PurchaseOrderLineData> lines) : base(id)
    {
        SupplierId = supplierId;
        DestinationWarehouseId = destinationWarehouseId;
        Status = PurchaseOrderStatus.Draft;

        Guard.NotNull(lines, nameof(lines));

        var duplicatedProduct = lines
            .GroupBy(line => line.ProductId)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicatedProduct is not null)
        {
            throw new PurchaseOrderDuplicateProductLineDomainException(Id, duplicatedProduct.Key);
        }

        foreach (var line in lines)
        {
            _lines.Add(new PurchaseOrderLine(line.Id, Id, line.ProductId, line.OrderedQuantity, line.UnitCost));
        }

        LinesCount = _lines.Count;
    }

    public void Confirm(DateTime now)
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new PurchaseOrderNotInDraftStateDomainException(Id, Status);
        }

        if (_lines.Count == 0)
        {
            throw new PurchaseOrderHasNoLinesDomainException(Id);
        }

        Status = PurchaseOrderStatus.Confirmed;
        AddLocalEvent(new PurchaseOrderConfirmedDomainEvent(Id, now));
    }

    internal void RegisterReceipt(Guid productId, Quantity quantity)
    {
        if (Status is not (PurchaseOrderStatus.Confirmed or PurchaseOrderStatus.PartiallyReceived))
        {
            throw new PurchaseOrderNotConfirmedDomainException(Id, Status);
        }

        Guard.Positive(quantity, nameof(quantity));

        var line = _lines.FirstOrDefault(l => l.ProductId == productId)
                   ?? throw new PurchaseOrderLineNotFoundDomainException(Id, productId);

        if (quantity > line.OutstandingQuantity)
        {
            throw new PurchaseOrderReceiptExceedsOrderedDomainException(Id, productId, quantity.Value, line.OutstandingQuantity.Value);
        }

        line.AddReceipt(quantity);
        Status = _lines.All(l => l.IsFullyReceived)
            ? PurchaseOrderStatus.FullyReceived
            : PurchaseOrderStatus.PartiallyReceived;
    }

    public void Cancel()
    {
        if (Status is not PurchaseOrderStatus.Draft and not PurchaseOrderStatus.Confirmed)
        {
            throw new PurchaseOrderCannotBeCancelledDomainException(Id, Status);
        }

        Status = PurchaseOrderStatus.Cancelled;
    }
}
