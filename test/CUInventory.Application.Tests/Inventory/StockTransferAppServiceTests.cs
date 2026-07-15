using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Inventory;

public abstract class StockTransferAppServiceTests<TStartupModule> : CUInventoryStockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private async Task<decimal> OnHandAsync(Guid warehouseId, Guid productId)
    {
        var balances = await InventoryBalanceAppService.GetListAsync(
            new GetInventoryBalanceListDto { WarehouseId = warehouseId, ProductId = productId });
        return balances.TotalCount == 0 ? 0m : balances.Items.Single().QuantityOnHand;
    }

    [Fact]
    public async Task Should_Create_Dispatch_And_Receive_Moving_Stock_Between_Warehouses()
    {
        var source = Guid.NewGuid();
        var destination = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(source, productId, quantity: 10m, unitCost: 5m);

        var transfer = await StockTransferAppService.CreateAsync(new CreateStockTransferDto
        {
            SourceWarehouseId = source,
            DestinationWarehouseId = destination,
            Lines = { new CreateStockTransferLineDto { ProductId = productId, Quantity = 4m } }
        });
        transfer.Status.ShouldBe(StockTransferStatus.Draft);

        var dispatched = await StockTransferAppService.DispatchAsync(transfer.Id, await StampOfTransferAsync(transfer.Id));
        dispatched.ShouldSatisfyAllConditions(
            () => dispatched.Status.ShouldBe(StockTransferStatus.Dispatched),
            () => dispatched.Allocations.ShouldHaveSingleItem(),
            () => dispatched.Allocations.Single().Quantity.ShouldBe(4m));

        // Source is deducted immediately on dispatch.
        (await OnHandAsync(source, productId)).ShouldBe(6m);
        // Destination has nothing until receipt.
        (await OnHandAsync(destination, productId)).ShouldBe(0m);

        var received = await StockTransferAppService.ReceiveAsync(transfer.Id, await StampOfTransferAsync(transfer.Id));
        received.Status.ShouldBe(StockTransferStatus.Received);

        (await OnHandAsync(destination, productId)).ShouldBe(4m);

        var destinationLots = await InventoryLotAppService.GetListAsync(
            new GetInventoryLotListDto { WarehouseId = destination, ProductId = productId });
        destinationLots.Items.ShouldHaveSingleItem().Source.ShouldBe(InventoryLotSource.TransferIn);
    }

    [Fact]
    public async Task Cancelling_After_Dispatch_Restores_The_Source_Stock()
    {
        var source = Guid.NewGuid();
        var destination = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(source, productId, quantity: 10m, unitCost: 5m);

        var transfer = await StockTransferAppService.CreateAsync(new CreateStockTransferDto
        {
            SourceWarehouseId = source,
            DestinationWarehouseId = destination,
            Lines = { new CreateStockTransferLineDto { ProductId = productId, Quantity = 4m } }
        });
        await StockTransferAppService.DispatchAsync(transfer.Id, await StampOfTransferAsync(transfer.Id));
        (await OnHandAsync(source, productId)).ShouldBe(6m);

        var cancelled = await StockTransferAppService.CancelAsync(transfer.Id, await StampOfTransferAsync(transfer.Id));
        cancelled.Status.ShouldBe(StockTransferStatus.Cancelled);

        // Conservation: everything dispatched comes back to the source.
        (await OnHandAsync(source, productId)).ShouldBe(10m);
        (await OnHandAsync(destination, productId)).ShouldBe(0m);
    }

    [Fact]
    public async Task Should_Reject_A_Transfer_Between_The_Same_Warehouse()
    {
        var warehouse = Guid.NewGuid();

        await Should.ThrowAsync<BusinessException>(
            () => StockTransferAppService.CreateAsync(new CreateStockTransferDto
            {
                SourceWarehouseId = warehouse,
                DestinationWarehouseId = warehouse,
                Lines = { new CreateStockTransferLineDto { ProductId = Guid.NewGuid(), Quantity = 1m } }
            }));
    }

    [Fact]
    public async Task Should_Reject_Dispatch_When_The_Source_Has_Insufficient_Stock()
    {
        var source = Guid.NewGuid();
        var destination = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(source, productId, quantity: 3m, unitCost: 5m);

        var transfer = await StockTransferAppService.CreateAsync(new CreateStockTransferDto
        {
            SourceWarehouseId = source,
            DestinationWarehouseId = destination,
            Lines = { new CreateStockTransferLineDto { ProductId = productId, Quantity = 5m } }
        });

        var stamp = await StampOfTransferAsync(transfer.Id);
        await Should.ThrowAsync<BusinessException>(() => StockTransferAppService.DispatchAsync(transfer.Id, stamp));
    }

    [Fact]
    public async Task Should_Reject_Receiving_Before_Dispatch()
    {
        var source = Guid.NewGuid();
        var destination = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(source, productId, quantity: 10m, unitCost: 5m);

        var transfer = await StockTransferAppService.CreateAsync(new CreateStockTransferDto
        {
            SourceWarehouseId = source,
            DestinationWarehouseId = destination,
            Lines = { new CreateStockTransferLineDto { ProductId = productId, Quantity = 4m } }
        });

        var stamp = await StampOfTransferAsync(transfer.Id);
        await Should.ThrowAsync<BusinessException>(() => StockTransferAppService.ReceiveAsync(transfer.Id, stamp));
    }
}
