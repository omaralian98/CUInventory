using System;
using System.Collections.Generic;
using System.Linq;
using CUInventory.Common;
using CUInventory.Inventory.Entities;
using CUInventory.Inventory.Events;
using CUInventory.Inventory.Exceptions;
using CUInventory.ValueObjects;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CUInventory.Inventory.Aggregates;

public class StockTransfer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid SourceWarehouseId { get; private set; }
    public Guid DestinationWarehouseId { get; private set; }
    public StockTransferStatus Status { get; private set; }
    public DateTime? DispatchedAt { get; private set; }
    public DateTime? ReceivedAt { get; private set; }
    public int LinesCount { get; private set; }
    public IReadOnlyCollection<StockTransferLine> Lines => _lines;
    public IReadOnlyCollection<TransferAllocation> Allocations => _allocations;
    private readonly List<StockTransferLine> _lines = [];
    private readonly List<TransferAllocation> _allocations = [];

    public Guid? TenantId { get; protected set; }
    protected StockTransfer()
    {
    }

    public StockTransfer(Guid id, Guid sourceWarehouseId, Guid destinationWarehouseId, IReadOnlyCollection<StockTransferLineData> lines) : base(id)
    {
        if (sourceWarehouseId == destinationWarehouseId)
        {
            throw new StockTransferSameWarehouseDomainException(sourceWarehouseId);
        }

        SourceWarehouseId = sourceWarehouseId;
        DestinationWarehouseId = destinationWarehouseId;
        Status = StockTransferStatus.Draft;

        Guard.NotNull(lines, nameof(lines));

        var duplicatedProduct = lines
            .GroupBy(line => line.ProductId)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicatedProduct is not null)
        {
            throw new StockTransferDuplicateProductLineDomainException(Id, duplicatedProduct.Key);
        }

        foreach (var line in lines)
        {
            _lines.Add(new StockTransferLine(line.Id, Id, line.ProductId, line.Quantity));
        }

        LinesCount = _lines.Count;
    }

    internal void AddAllocation(Guid allocationId, Guid sourceLotId, Guid productId, Guid? supplierId, Money unitCost, Quantity quantity)
    {
        _allocations.Add(new TransferAllocation(allocationId, Id, sourceLotId, productId, supplierId, unitCost, quantity));
    }

    internal void MarkDispatched(DateTime now)
    {
        if (Status != StockTransferStatus.Draft)
        {
            throw new StockTransferNotInDraftStateDomainException(Id, Status);
        }

        if (_lines.Count == 0)
        {
            throw new StockTransferHasNoLinesDomainException(Id);
        }

        Status = StockTransferStatus.Dispatched;
        DispatchedAt = now;
        AddLocalEvent(new TransferStartedDomainEvent(Id, now));
    }

    internal void MarkReceived(DateTime now)
    {
        if (Status != StockTransferStatus.Dispatched)
        {
            throw new StockTransferNotDispatchedDomainException(Id, Status);
        }

        Status = StockTransferStatus.Received;
        ReceivedAt = now;
        AddLocalEvent(new TransferCompletedDomainEvent(Id, now));
    }

    internal void MarkCancelled()
    {
        if (Status is StockTransferStatus.Received or StockTransferStatus.Cancelled)
        {
            throw new StockTransferCannotBeCancelledDomainException(Id, Status);
        }

        Status = StockTransferStatus.Cancelled;
    }
}
