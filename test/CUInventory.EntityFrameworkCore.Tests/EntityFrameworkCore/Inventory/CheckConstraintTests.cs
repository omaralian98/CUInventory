using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Inventory.Aggregates;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Inventory;

[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class CheckConstraintTests : CUInventoryEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task The_Database_Rejects_A_Negative_On_Hand_Quantity()
    {
        var balanceId = await SeedBalanceAsync();

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = await GetDbContextAsync();
            var affected = () => dbContext.Set<InventoryBalance>()
                .Where(b => b.Id == balanceId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.QuantityOnHand.Value, -1m));

            await affected.ShouldThrowAsync<DbException>();
        });
    }

    [Fact]
    public async Task The_Database_Rejects_Reserved_Exceeding_On_Hand()
    {
        var balanceId = await SeedBalanceAsync();

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = await GetDbContextAsync();
            var affected = () => dbContext.Set<InventoryBalance>()
                .Where(b => b.Id == balanceId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.QuantityReserved.Value, 5m));

            await affected.ShouldThrowAsync<DbException>();
        });
    }

    private async Task<Guid> SeedBalanceAsync()
    {
        var balance = new InventoryBalance(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await WithUnitOfWorkAsync(async () =>
        {
            var dbContext = await GetDbContextAsync();
            dbContext.Set<InventoryBalance>().Add(balance);
            await dbContext.SaveChangesAsync();
        });

        return balance.Id;
    }

    private async Task<CUInventoryDbContext> GetDbContextAsync()
    {
        var provider = GetRequiredService<IDbContextProvider<CUInventoryDbContext>>();
        return await provider.GetDbContextAsync();
    }
}
