using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Dtos;
using CUInventory.Inventory.Interfaces;
using CUInventory.Inventory.Repositories;
using CUInventory.Permissions;
using CUInventory.ValueObjects;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Inventory;

[Authorize(CUInventoryPermissions.StockTransfers.Default)]
public class StockTransferAppService :
    CUInventoryReadOnlyAppService<StockTransfer, StockTransferDto, StockTransferDto, Guid, GetStockTransferListDto>,
    IStockTransferAppService
{
    private readonly IStockTransferRepository _repository;
    private readonly IStockTransferManager _stockTransferManager;
    private readonly IInventoryBalanceManager _inventoryBalanceManager;
    private readonly IInventoryBalanceRepository _inventoryBalanceRepository;
    private readonly IInventoryLotRepository _inventoryLotRepository;

    public StockTransferAppService(
        IStockTransferRepository repository,
        IStockTransferManager stockTransferManager,
        IInventoryBalanceManager inventoryBalanceManager,
        IInventoryBalanceRepository inventoryBalanceRepository,
        IInventoryLotRepository inventoryLotRepository)
        : base(repository)
    {
        _repository = repository;
        _stockTransferManager = stockTransferManager;
        _inventoryBalanceManager = inventoryBalanceManager;
        _inventoryBalanceRepository = inventoryBalanceRepository;
        _inventoryLotRepository = inventoryLotRepository;

        GetPolicyName = CUInventoryPermissions.StockTransfers.Default;
        GetListPolicyName = CUInventoryPermissions.StockTransfers.Default;
    }

    public virtual async Task<StockTransferDto> CreateAsync(CreateStockTransferDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.StockTransfers.Create);

        var lines = input.Lines
            .Select(l => new StockTransferLineData(GuidGenerator.Create(), l.ProductId, new Quantity(l.Quantity)))
            .ToList();

        var transfer = new StockTransfer(
            GuidGenerator.Create(), input.SourceWarehouseId, input.DestinationWarehouseId, lines);

        await _repository.InsertAsync(transfer, autoSave: true);
        return await MapToGetOutputDtoAsync(transfer);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.StockTransfers.Delete);
        await _repository.DeleteAsync(id);
    }

    public virtual async Task<StockTransferDto> DispatchAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.StockTransfers.Dispatch);

        var transfer = await _repository.GetAsync(id);
        var productIds = transfer.Lines.Select(l => l.ProductId).Distinct().ToList();
        var sourceBalances = await GetBalancesAsync(transfer.SourceWarehouseId, productIds);
        var candidateLots = await GetLotsByWarehouseAndProductsAsync(transfer.SourceWarehouseId, productIds);

        await _stockTransferManager.DispatchAsync(transfer, sourceBalances, candidateLots);

        await _repository.UpdateAsync(transfer);
        await UpdateBalancesAsync(sourceBalances);
        await UpdateLotsAsync(candidateLots);

        return await MapToGetOutputDtoAsync(transfer);
    }

    public virtual async Task<StockTransferDto> ReceiveAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.StockTransfers.Receive);

        var transfer = await _repository.GetAsync(id);
        var productIds = transfer.Allocations.Select(a => a.ProductId).Distinct().ToList();
        var destinationBalances = await GetBalancesAsync(transfer.DestinationWarehouseId, productIds);

        var createdLots = await _stockTransferManager.ReceiveAsync(transfer, destinationBalances);

        await _repository.UpdateAsync(transfer);
        await UpdateBalancesAsync(destinationBalances);
        foreach (var lot in createdLots)
        {
            await _inventoryLotRepository.InsertAsync(lot);
        }

        return await MapToGetOutputDtoAsync(transfer);
    }

    public virtual async Task<StockTransferDto> CancelAsync(Guid id)
    {
        await CheckPolicyAsync(CUInventoryPermissions.StockTransfers.Cancel);

        var transfer = await _repository.GetAsync(id);

        var sourceBalances = new List<InventoryBalance>();
        var sourceLots = new List<InventoryLot>();
        if (transfer.Status == StockTransferStatus.Dispatched)
        {
            var productIds = transfer.Allocations.Select(a => a.ProductId).Distinct().ToList();
            sourceBalances = await GetBalancesAsync(transfer.SourceWarehouseId, productIds);
            var lotIds = transfer.Allocations.Select(a => a.SourceLotId).Distinct().ToList();
            sourceLots = await GetLotsByIdsAsync(lotIds);
        }

        await _stockTransferManager.CancelAsync(transfer, sourceBalances, sourceLots);

        await _repository.UpdateAsync(transfer);
        await UpdateBalancesAsync(sourceBalances);
        await UpdateLotsAsync(sourceLots);

        return await MapToGetOutputDtoAsync(transfer);
    }

    private async Task<List<InventoryBalance>> GetBalancesAsync(Guid warehouseId, IEnumerable<Guid> productIds)
    {
        var balances = new List<InventoryBalance>();
        foreach (var productId in productIds)
        {
            balances.Add(await _inventoryBalanceManager.GetOrCreateAsync(warehouseId, productId));
        }

        return balances;
    }

    private async Task<List<InventoryLot>> GetLotsByWarehouseAndProductsAsync(Guid warehouseId, List<Guid> productIds)
    {
        var query = (await _inventoryLotRepository.GetQueryableAsync())
            .Where(l => l.WarehouseId == warehouseId && productIds.Contains(l.ProductId) && l.RemainingQuantity.Value > 0);
        return await AsyncExecuter.ToListAsync(query);
    }

    private async Task<List<InventoryLot>> GetLotsByIdsAsync(List<Guid> lotIds)
    {
        var query = (await _inventoryLotRepository.GetQueryableAsync()).Where(l => lotIds.Contains(l.Id));
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
