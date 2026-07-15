using System;
using System.Threading.Tasks;
using CUInventory.Inventory.Interfaces;
using CUInventory.Inventory.Repositories;
using Shouldly;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Inventory;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class InventoryBalanceManagerTests : CUInventoryEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task GetOrCreateAsync_Creates_A_New_Balance_When_None_Exists()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var createdId = Guid.Empty;

        await WithUnitOfWorkAsync(async () =>
        {
            var manager = GetRequiredService<IInventoryBalanceManager>();
            var balance = await manager.GetOrCreateAsync(warehouseId, productId, lowStockThreshold: 7m);
            createdId = balance.Id;

            balance.ShouldSatisfyAllConditions(
                () => balance.WarehouseId.ShouldBe(warehouseId),
                () => balance.ProductId.ShouldBe(productId),
                () => balance.LowStockThreshold.ShouldBe(7m),
                () => balance.QuantityOnHand.Value.ShouldBe(0m),
                () => balance.QuantityReserved.Value.ShouldBe(0m));
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<IInventoryBalanceRepository>();
            var persisted = await repository.GetAsync(createdId);

            persisted.ShouldSatisfyAllConditions(
                () => persisted.WarehouseId.ShouldBe(warehouseId),
                () => persisted.ProductId.ShouldBe(productId),
                () => persisted.LowStockThreshold.ShouldBe(7m));
        });
    }

    [Fact]
    public async Task GetOrCreateAsync_Returns_The_Existing_Balance()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var firstId = Guid.Empty;

        await WithUnitOfWorkAsync(async () =>
        {
            var manager = GetRequiredService<IInventoryBalanceManager>();
            var balance = await manager.GetOrCreateAsync(warehouseId, productId, lowStockThreshold: 7m);
            firstId = balance.Id;
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var manager = GetRequiredService<IInventoryBalanceManager>();
            var balance = await manager.GetOrCreateAsync(warehouseId, productId, lowStockThreshold: 99m);

            balance.ShouldSatisfyAllConditions(
                () => balance.Id.ShouldBe(firstId),
                () => balance.LowStockThreshold.ShouldBe(7m));
        });
    }
}
