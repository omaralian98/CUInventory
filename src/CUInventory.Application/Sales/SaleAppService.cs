using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Repositories;
using CUInventory.Permissions;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Dtos;
using CUInventory.Sales.Exceptions;
using CUInventory.Sales.Interfaces;
using CUInventory.Sales.Repositories;
using CUInventory.Shared.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Sales;

[Authorize(CUInventoryPermissions.Sales.Default)]
public class SaleAppService :
    CUInventoryReadOnlyAppService<Sale, SaleDto, SaleDto, Guid, GetSaleListDto>,
    ISaleAppService
{
    private readonly ISaleRepository _repository;
    private readonly ISaleManager _saleManager;
    private readonly IInventoryBalanceRepository _inventoryBalanceRepository;
    private readonly IInventoryLotRepository _inventoryLotRepository;

    public SaleAppService(
        ISaleRepository repository,
        ISaleManager saleManager,
        IInventoryBalanceRepository inventoryBalanceRepository,
        IInventoryLotRepository inventoryLotRepository)
        : base(repository)
    {
        _repository = repository;
        _saleManager = saleManager;
        _inventoryBalanceRepository = inventoryBalanceRepository;
        _inventoryLotRepository = inventoryLotRepository;

        GetPolicyName = CUInventoryPermissions.Sales.Default;
        GetListPolicyName = CUInventoryPermissions.Sales.Default;
    }

    public virtual Task<SaleDto> CreateAsync(CreateSaleDto input)
    {
        return CreateCoreAsync(input);
    }

    private async Task<SaleDto> CreateCoreAsync(CreateSaleDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Sales.Create);

        var requests = input.Lines
            .Select(l => new SaleLineRequest(l.ProductId, l.Quantity, l.UnitPrice, l.Kind, l.WarehouseId, l.SupplierId, l.LotId))
            .ToList();

        var balances = await GetBalancesForRequestsAsync(requests);
        var candidateLots = await GetOpenLotsForRequestsAsync(requests);

        var sale = await _saleManager.CreateAsync(requests, balances, candidateLots);

        await _repository.InsertAsync(sale, autoSave: true);
        await UpdateTouchedAsync(sale, balances, candidateLots);

        return await MapToGetOutputDtoAsync(sale);
    }

    public virtual Task DeleteAsync(Guid id)
    {
        return DeleteCoreAsync(id);
    }

    private async Task DeleteCoreAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Sales.Delete);

        var sale = await _repository.GetAsync(id);
        if (sale.Status == SaleStatus.Confirmed)
        {
            throw new SaleCannotBeDeletedDomainException(sale.Id, sale.Status);
        }

        if (sale.Status == SaleStatus.Draft)
        {
            var pinnedLots = await GetLotsByIdsAsync(ReservedLotIds(sale));
            var balances = await GetBalancesForAllocationsAsync(sale);

            await _saleManager.CancelAsync(sale, balances, pinnedLots);
            await UpdateTouchedAsync(sale, balances, pinnedLots);
        }

        await _repository.DeleteAsync(sale);
    }

    public virtual Task<SaleDto> ConfirmAsync(Guid id, ConcurrencyStampDto input)
    {
        return ConfirmCoreAsync(id, input);
    }

    private async Task<SaleDto> ConfirmCoreAsync(Guid id, ConcurrencyStampDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Sales.Confirm);

        var sale = await _repository.GetAsync(id);
        sale.ConcurrencyStamp = input.ConcurrencyStamp;
        var pinnedLots = await GetLotsByIdsAsync(ReservedLotIds(sale));
        var balances = await GetBalancesForAllocationsAsync(sale);

        await _saleManager.ConfirmAsync(sale, balances, pinnedLots);

        await _repository.UpdateAsync(sale, autoSave: true);
        await UpdateTouchedAsync(sale, balances, pinnedLots);

        return await MapToGetOutputDtoAsync(sale);
    }

    public virtual Task<SaleDto> CancelAsync(Guid id, ConcurrencyStampDto input)
    {
        return CancelCoreAsync(id, input);
    }

    private async Task<SaleDto> CancelCoreAsync(Guid id, ConcurrencyStampDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Sales.Cancel);

        var sale = await _repository.GetAsync(id);
        sale.ConcurrencyStamp = input.ConcurrencyStamp;
        var pinnedLots = await GetLotsByIdsAsync(ReservedLotIds(sale));
        var balances = await GetBalancesForAllocationsAsync(sale);

        await _saleManager.CancelAsync(sale, balances, pinnedLots);

        await _repository.UpdateAsync(sale, autoSave: true);
        await UpdateTouchedAsync(sale, balances, pinnedLots);

        return await MapToGetOutputDtoAsync(sale);
    }

    protected override async Task<IQueryable<Sale>> CreateFilteredQueryAsync(GetSaleListDto input)
    {
        var query = await _repository.GetQueryableAsync();
        return query
            .WhereIf(input.Status.HasValue, s => s.Status == input.Status!.Value);
    }

    private async Task<List<InventoryBalance>> GetBalancesForRequestsAsync(List<SaleLineRequest> requests)
    {
        var openProductIds = ProductsWithoutWarehouse(requests);
        var scopedProductIds = ProductsWithWarehouse(requests);
        var scopedWarehouseIds = RequestedWarehouseIds(requests);

        var query = (await _inventoryBalanceRepository.GetQueryableAsync())
            .Where(b => openProductIds.Contains(b.ProductId) ||
                        (scopedProductIds.Contains(b.ProductId) && scopedWarehouseIds.Contains(b.WarehouseId)));
        return await AsyncExecuter.ToListAsync(query);
    }

    private async Task<List<InventoryLot>> GetOpenLotsForRequestsAsync(List<SaleLineRequest> requests)
    {
        var openProductIds = ProductsWithoutWarehouse(requests);
        var scopedProductIds = ProductsWithWarehouse(requests);
        var scopedWarehouseIds = RequestedWarehouseIds(requests);

        var query = (await _inventoryLotRepository.GetQueryableAsync())
            .Where(l => (openProductIds.Contains(l.ProductId) ||
                         (scopedProductIds.Contains(l.ProductId) && scopedWarehouseIds.Contains(l.WarehouseId))) &&
                        (l.RemainingQuantity.Value - l.ReservedQuantity.Value) > 0);
        return await AsyncExecuter.ToListAsync(query);
    }

    private static List<Guid> ProductsWithoutWarehouse(List<SaleLineRequest> requests) =>
        requests.Where(r => r.WarehouseId is null).Select(r => r.ProductId).Distinct().ToList();

    private static List<Guid> ProductsWithWarehouse(List<SaleLineRequest> requests) =>
        requests.Where(r => r.WarehouseId is not null).Select(r => r.ProductId).Distinct().ToList();

    private static List<Guid> RequestedWarehouseIds(List<SaleLineRequest> requests) =>
        requests.Where(r => r.WarehouseId is not null).Select(r => r.WarehouseId!.Value).Distinct().ToList();

    private static List<Guid> ReservedLotIds(Sale sale) =>
        sale.Lines
            .SelectMany(l => l.Allocations)
            .Where(a => a.IsReserved && a.InventoryLotId.HasValue)
            .Select(a => a.InventoryLotId!.Value)
            .Distinct()
            .ToList();

    private async Task<List<InventoryLot>> GetLotsByIdsAsync(List<Guid> lotIds)
    {
        var query = (await _inventoryLotRepository.GetQueryableAsync())
            .Where(l => lotIds.Contains(l.Id));
        return await AsyncExecuter.ToListAsync(query);
    }

    private async Task<List<InventoryBalance>> GetBalancesForAllocationsAsync(Sale sale)
    {
        var warehouseIds = sale.Lines.SelectMany(l => l.Allocations).Select(a => a.WarehouseId).Distinct().ToList();
        var productIds = sale.Lines.Select(l => l.ProductId).Distinct().ToList();

        var query = (await _inventoryBalanceRepository.GetQueryableAsync())
            .Where(b => warehouseIds.Contains(b.WarehouseId) && productIds.Contains(b.ProductId));
        return await AsyncExecuter.ToListAsync(query);
    }

    private async Task UpdateTouchedAsync(Sale sale, List<InventoryBalance> balances, List<InventoryLot> lots)
    {
        var touchedLotIds = sale.Lines
            .SelectMany(l => l.Allocations)
            .Where(a => a.InventoryLotId.HasValue)
            .Select(a => a.InventoryLotId!.Value)
            .ToHashSet();
        var touchedBalanceKeys = sale.Lines
            .SelectMany(l => l.Allocations.Select(a => (a.WarehouseId, l.ProductId)))
            .ToHashSet();

        foreach (var lot in lots.Where(l => touchedLotIds.Contains(l.Id)))
        {
            await _inventoryLotRepository.UpdateAsync(lot);
        }

        foreach (var balance in balances.Where(b => touchedBalanceKeys.Contains((b.WarehouseId, b.ProductId))))
        {
            await _inventoryBalanceRepository.UpdateAsync(balance);
        }
    }
}
