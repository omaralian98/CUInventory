using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Repositories;
using CUInventory.Permissions;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Dtos;
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

    public virtual async Task<SaleDto> CreateAsync(CreateSaleDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Sales.Create);

        var requests = input.Lines
            .Select(l => new SaleLineRequest(l.ProductId, l.Quantity, l.UnitPrice, l.Kind, l.WarehouseId, l.SupplierId, l.LotId))
            .ToList();

        var productIds = requests.Select(r => r.ProductId).Distinct().ToList();
        var balances = await GetBalancesForProductsAsync(productIds);
        var candidateLots = await GetLotsForProductsAsync(productIds);

        var sale = await _saleManager.CreateAsync(requests, balances, candidateLots);

        await _repository.InsertAsync(sale);
        await UpdateBalancesAsync(balances);

        return await MapToGetOutputDtoAsync(sale);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Sales.Delete);
        await _repository.DeleteAsync(id);
    }

    public virtual async Task<SaleDto> ConfirmAsync(Guid id, ConcurrencyStampDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Sales.Confirm);

        var sale = await _repository.GetAsync(id);
        sale.ConcurrencyStamp = input.ConcurrencyStamp;
        var productIds = sale.Lines.Select(l => l.ProductId).Distinct().ToList();
        var balances = await GetBalancesForProductsAsync(productIds);
        var candidateLots = await GetLotsForProductsAsync(productIds);

        await _saleManager.ConfirmAsync(sale, balances, candidateLots);

        await _repository.UpdateAsync(sale, autoSave: true);
        await UpdateBalancesAsync(balances);
        await UpdateLotsAsync(candidateLots);

        return await MapToGetOutputDtoAsync(sale);
    }

    public virtual async Task<SaleDto> CancelAsync(Guid id, ConcurrencyStampDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.Sales.Cancel);

        var sale = await _repository.GetAsync(id);
        sale.ConcurrencyStamp = input.ConcurrencyStamp;
        var productIds = sale.Lines.Select(l => l.ProductId).Distinct().ToList();
        var balances = await GetBalancesForProductsAsync(productIds);

        await _saleManager.CancelAsync(sale, balances);

        await _repository.UpdateAsync(sale, autoSave: true);
        await UpdateBalancesAsync(balances);

        return await MapToGetOutputDtoAsync(sale);
    }

    protected override async Task<IQueryable<Sale>> CreateFilteredQueryAsync(GetSaleListDto input)
    {
        var query = await _repository.GetQueryableAsync();
        return query
            .WhereIf(input.Status.HasValue, s => s.Status == input.Status!.Value);
    }

    private async Task<List<InventoryBalance>> GetBalancesForProductsAsync(List<Guid> productIds)
    {
        var query = (await _inventoryBalanceRepository.GetQueryableAsync())
            .Where(b => productIds.Contains(b.ProductId));
        return await AsyncExecuter.ToListAsync(query);
    }

    private async Task<List<InventoryLot>> GetLotsForProductsAsync(List<Guid> productIds)
    {
        var query = (await _inventoryLotRepository.GetQueryableAsync())
            .Where(l => productIds.Contains(l.ProductId) && l.RemainingQuantity.Value > 0);
        return await AsyncExecuter.ToListAsync(query);
    }

    private async Task UpdateBalancesAsync(List<InventoryBalance> balances)
    {
        foreach (var balance in balances)
        {
            await _inventoryBalanceRepository.UpdateAsync(balance);
        }
    }

    private async Task UpdateLotsAsync(List<InventoryLot> lots)
    {
        foreach (var lot in lots)
        {
            await _inventoryLotRepository.UpdateAsync(lot);
        }
    }
}
