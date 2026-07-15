using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Permissions;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Dtos;
using CUInventory.Procurement.Repositories;
using CUInventory.Shared.Dtos;
using CUInventory.ValueObjects;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Procurement;

[Authorize(CUInventoryPermissions.PurchaseOrders.Default)]
public class PurchaseOrderAppService :
    CUInventoryCrudAppService<PurchaseOrder, PurchaseOrderDto, PurchaseOrderDto, Guid, GetPurchaseOrderListDto, CreatePurchaseOrderDto>,
    IPurchaseOrderAppService
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly ISupplierRepository _supplierRepository;

    public PurchaseOrderAppService(
        IPurchaseOrderRepository repository,
        ISupplierRepository supplierRepository)
        : base(repository)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;

        GetPolicyName = CUInventoryPermissions.PurchaseOrders.Default;
        GetListPolicyName = CUInventoryPermissions.PurchaseOrders.Default;
        CreatePolicyName = CUInventoryPermissions.PurchaseOrders.Create;
        DeletePolicyName = CUInventoryPermissions.PurchaseOrders.Delete;
    }

    public override async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto input)
    {
        await CheckCreatePolicyAsync();

        var lines = input.Lines
            .Select(l => new PurchaseOrderLineData(
                GuidGenerator.Create(), l.ProductId, new Quantity(l.OrderedQuantity), new Money(l.UnitCost)))
            .ToList();

        var purchaseOrder = new PurchaseOrder(
            GuidGenerator.Create(), input.SupplierId, input.DestinationWarehouseId, lines);

        await _repository.InsertAsync(purchaseOrder, autoSave: true);
        return await MapToGetOutputDtoAsync(purchaseOrder);
    }

    public virtual Task<PurchaseOrderDto> ConfirmAsync(Guid id, ConcurrencyStampDto input)
    {
        return ConfirmCoreAsync(id, input);
    }

    private async Task<PurchaseOrderDto> ConfirmCoreAsync(Guid id, ConcurrencyStampDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.PurchaseOrders.Confirm);

        var purchaseOrder = await _repository.GetAsync(id);
        purchaseOrder.ConcurrencyStamp = input.ConcurrencyStamp;
        purchaseOrder.Confirm(Clock.Now);

        await _repository.UpdateAsync(purchaseOrder, autoSave: true);
        return await MapToGetOutputDtoAsync(purchaseOrder);
    }

    public virtual Task<PurchaseOrderDto> CancelAsync(Guid id, ConcurrencyStampDto input)
    {
        return CancelCoreAsync(id, input);
    }

    private async Task<PurchaseOrderDto> CancelCoreAsync(Guid id, ConcurrencyStampDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.PurchaseOrders.Cancel);

        var purchaseOrder = await _repository.GetAsync(id);
        purchaseOrder.ConcurrencyStamp = input.ConcurrencyStamp;
        purchaseOrder.Cancel();

        await _repository.UpdateAsync(purchaseOrder, autoSave: true);
        return await MapToGetOutputDtoAsync(purchaseOrder);
    }

    protected override async Task<IQueryable<PurchaseOrder>> CreateFilteredQueryAsync(GetPurchaseOrderListDto input)
    {
        var query = await _repository.GetQueryableAsync();
        var suppliers = await _supplierRepository.GetQueryableAsync();
        return query
            .WhereIf(input.SupplierId.HasValue, po => po.SupplierId == input.SupplierId!.Value)
            .WhereIf(input.DestinationWarehouseId.HasValue, po => po.DestinationWarehouseId == input.DestinationWarehouseId!.Value)
            .WhereIf(input.Status.HasValue, po => po.Status == input.Status!.Value)
            .WhereIf(input.Statuses is { Count: > 0 }, po => input.Statuses!.Contains(po.Status))
            .WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                po => suppliers.Any(s => s.Id == po.SupplierId && s.Name.Contains(input.Filter!)));
    }
}
