using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Dtos;
using CUInventory.Inventory.Exceptions;
using CUInventory.Inventory.Interfaces;
using CUInventory.Inventory.Repositories;
using CUInventory.Permissions;
using CUInventory.Shared.Dtos;
using CUInventory.ValueObjects;
using Microsoft.AspNetCore.Authorization;

namespace CUInventory.Inventory;

[Authorize(CUInventoryPermissions.StockTransfers.Default)]
public class StockTransferAppService :
    CUInventoryCrudAppService<StockTransfer, StockTransferDto, StockTransferDto, Guid, GetStockTransferListDto, CreateStockTransferDto>,
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
        CreatePolicyName = CUInventoryPermissions.StockTransfers.Create;
        DeletePolicyName = CUInventoryPermissions.StockTransfers.Delete;
    }

    public override async Task<StockTransferDto> CreateAsync(CreateStockTransferDto input)
    {
        await CheckCreatePolicyAsync();

        var lines = input.Lines
            .Select(l => new StockTransferLineData(GuidGenerator.Create(), l.ProductId, new Quantity(l.Quantity)))
            .ToList();

        var transfer = new StockTransfer(
            GuidGenerator.Create(), input.SourceWarehouseId, input.DestinationWarehouseId, lines);

        await _repository.InsertAsync(transfer, autoSave: true);
        return await MapToGetOutputDtoAsync(transfer);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var transfer = await _repository.GetAsync(id);
        if (transfer.Status is not (StockTransferStatus.Draft or StockTransferStatus.Cancelled))
        {
            throw new StockTransferCannotBeDeletedDomainException(transfer.Id, transfer.Status);
        }

        await _repository.DeleteAsync(transfer);
    }

    public virtual Task<StockTransferDto> DispatchAsync(Guid id, ConcurrencyStampDto input)
    {
        return DispatchCoreAsync(id, input);
    }

    private async Task<StockTransferDto> DispatchCoreAsync(Guid id, ConcurrencyStampDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.StockTransfers.Dispatch);

        var transfer = await _repository.GetAsync(id);
        transfer.ConcurrencyStamp = input.ConcurrencyStamp;
        var productIds = transfer.Lines.Select(l => l.ProductId).Distinct().ToList();
        var sourceBalances = await GetBalancesAsync(transfer.SourceWarehouseId, productIds);
        var candidateLots = await GetLotsByWarehouseAndProductsAsync(transfer.SourceWarehouseId, productIds);

        await _stockTransferManager.DispatchAsync(transfer, sourceBalances, candidateLots);

        await _repository.UpdateAsync(transfer, autoSave: true);
        await UpdateBalancesAsync(sourceBalances);
        var consumedLotIds = transfer.Allocations.Select(a => a.SourceLotId).ToHashSet();
        await UpdateLotsAsync(candidateLots.Where(l => consumedLotIds.Contains(l.Id)).ToList());

        return await MapToGetOutputDtoAsync(transfer);
    }

    public virtual Task<StockTransferDto> ReceiveAsync(Guid id, ConcurrencyStampDto input)
    {
        return ReceiveCoreAsync(id, input);
    }

    private async Task<StockTransferDto> ReceiveCoreAsync(Guid id, ConcurrencyStampDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.StockTransfers.Receive);

        var transfer = await _repository.GetAsync(id);
        transfer.ConcurrencyStamp = input.ConcurrencyStamp;
        var productIds = transfer.Allocations.Select(a => a.ProductId).Distinct().ToList();
        var destinationBalances = await GetBalancesAsync(transfer.DestinationWarehouseId, productIds);
        var sourceLots = await GetLotsByIdsAsync(transfer.Allocations.Select(a => a.SourceLotId).Distinct().ToList());

        var createdLots = await _stockTransferManager.ReceiveAsync(transfer, destinationBalances, sourceLots);

        await _repository.UpdateAsync(transfer, autoSave: true);
        await UpdateBalancesAsync(destinationBalances);
        foreach (var lot in createdLots)
        {
            await _inventoryLotRepository.InsertAsync(lot);
        }

        return await MapToGetOutputDtoAsync(transfer);
    }

    public virtual Task<StockTransferDto> CancelAsync(Guid id, ConcurrencyStampDto input)
    {
        return CancelCoreAsync(id, input);
    }

    private async Task<StockTransferDto> CancelCoreAsync(Guid id, ConcurrencyStampDto input)
    {
        await CheckPolicyAsync(CUInventoryPermissions.StockTransfers.Cancel);

        var transfer = await _repository.GetAsync(id);
        transfer.ConcurrencyStamp = input.ConcurrencyStamp;

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

        await _repository.UpdateAsync(transfer, autoSave: true);
        await UpdateBalancesAsync(sourceBalances);
        await UpdateLotsAsync(sourceLots);

        return await MapToGetOutputDtoAsync(transfer);
    }

    protected override async Task<IQueryable<StockTransfer>> CreateFilteredQueryAsync(GetStockTransferListDto input)
    {
        var query = await _repository.GetQueryableAsync();
        return query
            .WhereIf(input.SourceWarehouseId.HasValue, t => t.SourceWarehouseId == input.SourceWarehouseId!.Value)
            .WhereIf(input.DestinationWarehouseId.HasValue, t => t.DestinationWarehouseId == input.DestinationWarehouseId!.Value)
            .WhereIf(input.Status.HasValue, t => t.Status == input.Status!.Value);
    }

    private async Task<List<InventoryBalance>> GetBalancesAsync(Guid warehouseId, IEnumerable<Guid> productIds)
    {
        var ids = productIds.ToList();
        var query = (await _inventoryBalanceRepository.GetQueryableAsync())
            .Where(b => b.WarehouseId == warehouseId && ids.Contains(b.ProductId));
        var balances = await AsyncExecuter.ToListAsync(query);

        foreach (var productId in ids.Except(balances.Select(b => b.ProductId)))
        {
            balances.Add(await _inventoryBalanceManager.GetOrCreateAsync(warehouseId, productId));
        }

        return balances;
    }

    private async Task<List<InventoryLot>> GetLotsByWarehouseAndProductsAsync(Guid warehouseId, List<Guid> productIds)
    {
        var query = (await _inventoryLotRepository.GetQueryableAsync())
            .Where(l => l.WarehouseId == warehouseId && productIds.Contains(l.ProductId) &&
                        (l.RemainingQuantity.Value - l.ReservedQuantity.Value) > 0);
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
