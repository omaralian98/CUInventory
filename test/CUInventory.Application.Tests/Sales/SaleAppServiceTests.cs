using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory;
using CUInventory.Inventory.Dtos;
using CUInventory.Sales.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Sales;

public abstract class SaleAppServiceTests<TStartupModule> : CUInventoryStockTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private async Task<InventoryBalanceDto> BalanceAsync(Guid warehouseId, Guid productId)
    {
        var balances = await InventoryBalanceAppService.GetListAsync(
            new GetInventoryBalanceListDto { WarehouseId = warehouseId, ProductId = productId });
        return balances.Items.Single();
    }

    [Fact]
    public async Task Create_Reserves_Stock_Without_Reducing_On_Hand()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);

        var sale = await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 4m, UnitPrice = 20m, WarehouseId = warehouseId } }
        });

        sale.Status.ShouldBe(SaleStatus.Draft);
        sale.Lines.Single().Allocations.ShouldAllBe(a => a.IsReserved);

        var balance = await BalanceAsync(warehouseId, productId);
        balance.ShouldSatisfyAllConditions(
            () => balance.QuantityOnHand.ShouldBe(10m),
            () => balance.QuantityReserved.ShouldBe(4m),
            () => balance.QuantityAvailable.ShouldBe(6m));
    }

    [Fact]
    public async Task Confirm_Consumes_On_Hand_And_Binds_Lots()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);

        var sale = await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 4m, UnitPrice = 20m, WarehouseId = warehouseId } }
        });

        var confirmed = await SaleAppService.ConfirmAsync(sale.Id, await StampOfSaleAsync(sale.Id));

        confirmed.ShouldSatisfyAllConditions(
            () => confirmed.Status.ShouldBe(SaleStatus.Confirmed),
            () => confirmed.ConfirmedAt.ShouldNotBeNull(),
            () => confirmed.Lines.Single().Allocations.ShouldAllBe(a => !a.IsReserved && a.InventoryLotId != null));

        var balance = await BalanceAsync(warehouseId, productId);
        balance.ShouldSatisfyAllConditions(
            () => balance.QuantityOnHand.ShouldBe(6m),
            () => balance.QuantityReserved.ShouldBe(0m),
            () => balance.QuantityAvailable.ShouldBe(6m));

        var lots = await InventoryLotAppService.GetListAsync(
            new GetInventoryLotListDto { WarehouseId = warehouseId, ProductId = productId });
        lots.Items.Single().RemainingQuantity.ShouldBe(6m);
    }

    [Fact]
    public async Task Cancel_Releases_The_Reservation()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);

        var sale = await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 4m, UnitPrice = 20m, WarehouseId = warehouseId } }
        });

        var cancelled = await SaleAppService.CancelAsync(sale.Id, await StampOfSaleAsync(sale.Id));
        cancelled.Status.ShouldBe(SaleStatus.Cancelled);

        var balance = await BalanceAsync(warehouseId, productId);
        balance.ShouldSatisfyAllConditions(
            () => balance.QuantityOnHand.ShouldBe(10m),
            () => balance.QuantityReserved.ShouldBe(0m),
            () => balance.QuantityAvailable.ShouldBe(10m));
    }

    [Fact]
    public async Task Cannot_Cancel_A_Confirmed_Sale()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);

        var sale = await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 4m, UnitPrice = 20m, WarehouseId = warehouseId } }
        });
        await SaleAppService.ConfirmAsync(sale.Id, await StampOfSaleAsync(sale.Id));

        var stamp = await StampOfSaleAsync(sale.Id);
        await Should.ThrowAsync<BusinessException>(() => SaleAppService.CancelAsync(sale.Id, stamp));
    }

    [Fact]
    public async Task Confirm_With_A_Stale_Stamp_Still_Fails_With_A_Concurrency_Error()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);

        var sale = await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 4m, UnitPrice = 20m, WarehouseId = warehouseId } }
        });

        var staleStamp = new Shared.Dtos.ConcurrencyStampDto { ConcurrencyStamp = Guid.NewGuid().ToString("N") };
        await Should.ThrowAsync<AbpDbConcurrencyException>(() => SaleAppService.ConfirmAsync(sale.Id, staleStamp));
    }

    [Fact]
    public async Task Create_Rejects_A_Sale_That_Exceeds_Available_Stock()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 3m);

        await Should.ThrowAsync<BusinessException>(
            () => SaleAppService.CreateAsync(new CreateSaleDto
            {
                Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 5m, UnitPrice = 20m, WarehouseId = warehouseId } }
            }));

        // Nothing was reserved on the failed attempt.
        var balance = await BalanceAsync(warehouseId, productId);
        balance.QuantityReserved.ShouldBe(0m);
    }

    [Fact]
    public async Task Confirm_Allocates_Across_Multiple_Fifo_Lots()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        // Two receipts create two lots (6 + 6) for the same balance.
        await SeedStockAsync(warehouseId, productId, quantity: 6m, unitCost: 5m);
        await SeedStockAsync(warehouseId, productId, quantity: 6m, unitCost: 7m);

        var sale = await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines = { new CreateSaleLineDto { ProductId = productId, Quantity = 10m, UnitPrice = 20m, WarehouseId = warehouseId } }
        });

        var confirmed = await SaleAppService.ConfirmAsync(sale.Id, await StampOfSaleAsync(sale.Id));

        var allocations = confirmed.Lines.Single().Allocations;
        allocations.ShouldSatisfyAllConditions(
            () => allocations.Count.ShouldBeGreaterThanOrEqualTo(2),
            () => allocations.Sum(a => a.Quantity).ShouldBe(10m));

        var balance = await BalanceAsync(warehouseId, productId);
        balance.QuantityOnHand.ShouldBe(2m);
    }

    [Fact]
    public async Task Confirm_Binds_The_Specific_Lot_Requested()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var lotId = await SeedStockAsync(warehouseId, productId, quantity: 10m);

        var sale = await SaleAppService.CreateAsync(new CreateSaleDto
        {
            Lines =
            {
                new CreateSaleLineDto
                {
                    ProductId = productId,
                    Quantity = 4m,
                    UnitPrice = 20m,
                    WarehouseId = warehouseId,
                    Kind = AllocationStrategyKind.SpecificLot,
                    LotId = lotId
                }
            }
        });

        var confirmed = await SaleAppService.ConfirmAsync(sale.Id, await StampOfSaleAsync(sale.Id));

        confirmed.Lines.Single().Allocations.ShouldAllBe(a => a.InventoryLotId == lotId);
    }
}
