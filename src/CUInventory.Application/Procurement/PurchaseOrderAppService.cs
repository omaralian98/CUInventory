using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Permissions;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Dtos;
using CUInventory.Procurement.Repositories;
using CUInventory.ValueObjects;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Procurement;

[Authorize(CUInventoryPermissions.PurchaseOrders.Default)]
public class PurchaseOrderAppService :
    CUInventoryReadOnlyAppService<PurchaseOrder, PurchaseOrderDto, PurchaseOrderDto, Guid, GetPurchaseOrderListDto>,
    IPurchaseOrderAppService
{
    private readonly IPurchaseOrderRepository _repository;

    public PurchaseOrderAppService(IPurchaseOrderRepository repository)
        : base(repository)
    {
        _repository = repository;

        GetPolicyName = CUInventoryPermissions.PurchaseOrders.Default;
        GetListPolicyName = CUInventoryPermissions.PurchaseOrders.Default;
    }

    public virtual async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.PurchaseOrders.Create);

        var lines = input.Lines
            .Select(l => new PurchaseOrderLineData(
                GuidGenerator.Create(), l.ProductId, new Quantity(l.OrderedQuantity), new Money(l.UnitCost)))
            .ToList();

        var purchaseOrder = new PurchaseOrder(
            GuidGenerator.Create(), input.SupplierId, input.DestinationWarehouseId, lines);

        await _repository.InsertAsync(purchaseOrder, autoSave: true);
        return await MapToGetOutputDtoAsync(purchaseOrder);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.PurchaseOrders.Delete);
        await _repository.DeleteAsync(id);
    }

    public virtual async Task<PurchaseOrderDto> ConfirmAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.PurchaseOrders.Confirm);

        var purchaseOrder = await _repository.GetAsync(id);
        purchaseOrder.Confirm(Clock.Now);

        await _repository.UpdateAsync(purchaseOrder, autoSave: true);
        return await MapToGetOutputDtoAsync(purchaseOrder);
    }

    public virtual async Task<PurchaseOrderDto> CancelAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.PurchaseOrders.Cancel);

        var purchaseOrder = await _repository.GetAsync(id);
        purchaseOrder.Cancel();

        await _repository.UpdateAsync(purchaseOrder, autoSave: true);
        return await MapToGetOutputDtoAsync(purchaseOrder);
    }
}
