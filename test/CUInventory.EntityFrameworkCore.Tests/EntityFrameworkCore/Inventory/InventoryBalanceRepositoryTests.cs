using System;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using CUInventory.Inventory.Repositories;
using Shouldly;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Inventory;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class InventoryBalanceRepositoryTests : CUInventoryEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task InsertOrGetAsync_Returns_The_Existing_Row_When_The_Unique_Index_Fires()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var existingId = Guid.Empty;

        await WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<IInventoryBalanceRepository>();
            var existing = await repository.InsertOrGetAsync(new InventoryBalance(Guid.NewGuid(), warehouseId, productId));
            existingId = existing.Id;
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<IInventoryBalanceRepository>();
            var result = await repository.InsertOrGetAsync(new InventoryBalance(Guid.NewGuid(), warehouseId, productId));

            result.Id.ShouldBe(existingId);
        });
    }
}
