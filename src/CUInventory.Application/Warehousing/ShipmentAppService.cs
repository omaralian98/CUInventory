using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Interfaces;
using CUInventory.Inventory.Repositories;
using CUInventory.Permissions;
using CUInventory.Procurement.Repositories;
using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Dtos;
using CUInventory.Warehousing.Interfaces;
using CUInventory.Warehousing.Repositories;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Warehousing;

[Authorize(CUInventoryPermissions.Shipments.Default)]
public class ShipmentAppService :
    CUInventoryReadOnlyAppService<Shipment, ShipmentDto, ShipmentDto, Guid, GetShipmentListDto>,
    IShipmentAppService
{
    private readonly IShipmentRepository _repository;
    private readonly IShipmentManager _shipmentManager;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IInventoryBalanceManager _inventoryBalanceManager;
    private readonly IInventoryBalanceRepository _inventoryBalanceRepository;
    private readonly IInventoryLotRepository _inventoryLotRepository;

    public ShipmentAppService(
        IShipmentRepository repository,
        IShipmentManager shipmentManager,
        IPurchaseOrderRepository purchaseOrderRepository,
        IInventoryBalanceManager inventoryBalanceManager,
        IInventoryBalanceRepository inventoryBalanceRepository,
        IInventoryLotRepository inventoryLotRepository)
        : base(repository)
    {
        _repository = repository;
        _shipmentManager = shipmentManager;
        _purchaseOrderRepository = purchaseOrderRepository;
        _inventoryBalanceManager = inventoryBalanceManager;
        _inventoryBalanceRepository = inventoryBalanceRepository;
        _inventoryLotRepository = inventoryLotRepository;

        GetPolicyName = CUInventoryPermissions.Shipments.Default;
        GetListPolicyName = CUInventoryPermissions.Shipments.Default;
    }

    public virtual async Task<ShipmentDto> CreateAsync(CreateShipmentDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Shipments.Create);

        var lines = input.Lines
            .Select(l => new ShipmentLineData(
                GuidGenerator.Create(), l.ProductId, new Quantity(l.Quantity), new Money(l.UnitCost)))
            .ToList();

        var shipment = new Shipment(
            GuidGenerator.Create(), input.PurchaseOrderId, input.SupplierId, input.DestinationWarehouseId, lines);

        await _repository.InsertAsync(shipment, autoSave: true);
        return await MapToGetOutputDtoAsync(shipment);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Shipments.Delete);
        await _repository.DeleteAsync(id);
    }

    public virtual async Task<ShipmentDto> DispatchAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Shipments.Dispatch);

        var shipment = await _repository.GetAsync(id);
        shipment.Dispatch(Clock.Now);

        await _repository.UpdateAsync(shipment, autoSave: true);
        return await MapToGetOutputDtoAsync(shipment);
    }

    public virtual async Task<ShipmentDto> ReceiveAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Shipments.Receive);

        var shipment = await _repository.GetAsync(id);
        var purchaseOrder = await _purchaseOrderRepository.GetAsync(shipment.PurchaseOrderId);

        var balances = new List<InventoryBalance>();
        foreach (var productId in shipment.Lines.Select(l => l.ProductId).Distinct())
        {
            balances.Add(await _inventoryBalanceManager.GetOrCreateAsync(shipment.DestinationWarehouseId, productId));
        }

        var createdLots = await _shipmentManager.ReceiveAsync(shipment, purchaseOrder, balances);

        await _repository.UpdateAsync(shipment);
        await _purchaseOrderRepository.UpdateAsync(purchaseOrder);
        foreach (var balance in balances)
        {
            await _inventoryBalanceRepository.UpdateAsync(balance);
        }

        foreach (var lot in createdLots)
        {
            await _inventoryLotRepository.InsertAsync(lot);
        }

        return await MapToGetOutputDtoAsync(shipment);
    }
}
