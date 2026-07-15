using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Dtos;
using CUInventory.Inventory.Repositories;
using CUInventory.Sales;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Interfaces;
using CUInventory.Sales.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Uow;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Inventory;

/// <summary>
/// Verifies the system behaves correctly when two operations compete for the same limited stock.
/// The application services load a balance, mutate it in memory and save it back with no retry loop,
/// relying solely on the optimistic <c>ConcurrencyStamp</c>. This test reproduces that race
/// deterministically: two units of work load the same balance row (same stamp) before either saves,
/// both reserve the last units, and the second save must fail rather than oversell.
/// </summary>
[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class InventoryConcurrencyTests : CUInventoryStockTestBase<CUInventoryEntityFrameworkCoreTestModule>
{
    [Fact]
    public async Task Two_Concurrent_Reservations_For_The_Same_Limited_Stock_Only_One_Succeeds()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);
        var balanceId = await FindBalanceIdAsync(warehouseId, productId);

        using var scopeWinner = ServiceProvider.CreateScope();
        using var scopeLoser = ServiceProvider.CreateScope();

        var loserUowManager = scopeLoser.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var winnerUowManager = scopeWinner.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();

        // The "loser" opens its unit of work first and reads the balance and lots (stamp S). It will try to save last.
        using (var loserUow = loserUowManager.Begin(new AbpUnitOfWorkOptions(), requiresNew: true))
        {
            var loserBalanceRepository = scopeLoser.ServiceProvider.GetRequiredService<IInventoryBalanceRepository>();
            var loserBalance = await loserBalanceRepository.GetAsync(balanceId);
            var loserLots = await LoadLotsAsync(scopeLoser, warehouseId, productId);

            // While the loser still holds its stale snapshot, the winner reserves and commits the whole 10 units.
            using (var winnerUow = winnerUowManager.Begin(new AbpUnitOfWorkOptions(), requiresNew: true))
            {
                var winnerBalanceRepository = scopeWinner.ServiceProvider.GetRequiredService<IInventoryBalanceRepository>();
                var winnerBalance = await winnerBalanceRepository.GetAsync(balanceId);
                var winnerLots = await LoadLotsAsync(scopeWinner, warehouseId, productId);

                await ReserveAsync(scopeWinner, winnerBalance, winnerLots, productId, warehouseId, 10m);
                await winnerUow.CompleteAsync();
            }

            // The loser reserved against the now-stale balance and lots; committing must be rejected, not silently oversell.
            await ReserveAsync(scopeLoser, loserBalance, loserLots, productId, warehouseId, 10m);
            await Should.ThrowAsync<AbpDbConcurrencyException>(() => loserUow.CompleteAsync());
        }

        // Exactly one sale survived: the loser's insert rolled back with its failed balance update.
        await WithUnitOfWorkAsync(async () =>
        {
            var saleRepository = GetRequiredService<ISaleRepository>();
            var sales = await saleRepository.GetListAsync(s => s.Lines.Any(l => l.ProductId == productId));
            sales.Count.ShouldBe(1);
        });

        // And the stock was not oversold: only the winner's 10 units are reserved.
        var finalBalance = await InventoryBalanceAppService.GetAsync(balanceId);
        finalBalance.ShouldSatisfyAllConditions(
            () => finalBalance.QuantityOnHand.ShouldBe(10m),
            () => finalBalance.QuantityReserved.ShouldBe(10m),
            () => finalBalance.QuantityAvailable.ShouldBe(0m));
    }

    private static async Task<List<InventoryLot>> LoadLotsAsync(IServiceScope scope, Guid warehouseId, Guid productId)
    {
        var lotRepository = scope.ServiceProvider.GetRequiredService<IInventoryLotRepository>();
        return await lotRepository.GetListAsync(l => l.WarehouseId == warehouseId && l.ProductId == productId);
    }

    private static async Task<Sale> ReserveAsync(
        IServiceScope scope,
        InventoryBalance balance,
        List<InventoryLot> lots,
        Guid productId,
        Guid warehouseId,
        decimal quantity)
    {
        var saleManager = scope.ServiceProvider.GetRequiredService<ISaleManager>();
        var saleRepository = scope.ServiceProvider.GetRequiredService<ISaleRepository>();
        var balanceRepository = scope.ServiceProvider.GetRequiredService<IInventoryBalanceRepository>();
        var lotRepository = scope.ServiceProvider.GetRequiredService<IInventoryLotRepository>();

        var sale = await saleManager.CreateAsync(
            new List<SaleLineRequest> { new(productId, quantity, 20m, WarehouseId: warehouseId) },
            new List<InventoryBalance> { balance },
            lots);

        await saleRepository.InsertAsync(sale);
        await balanceRepository.UpdateAsync(balance);
        foreach (var lot in lots)
        {
            await lotRepository.UpdateAsync(lot);
        }

        return sale;
    }
}
