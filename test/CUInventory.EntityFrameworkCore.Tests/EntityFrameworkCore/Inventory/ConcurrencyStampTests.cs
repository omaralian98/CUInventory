using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Catalog;
using CUInventory.Catalog.Dtos;
using CUInventory.Inventory;
using CUInventory.Inventory.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Volo.Abp.Data;
using Xunit;

namespace CUInventory.EntityFrameworkCore.Inventory;

/// <summary>
/// Races two updates that both carry the same <c>ConcurrencyStamp</c>, each in its own scope with its
/// own unit of work. The interleaving is serialized deterministically (the shared in-memory SQLite
/// connection is not thread-safe): the first save bumps the stored stamp, so the second's conditional
/// UPDATE matches zero rows and must surface <c>AbpDbConcurrencyException</c> - exactly one winner.
/// </summary>
[Collection(CUInventoryTestConsts.CollectionDefinitionName)]
public class ConcurrencyStampTests : CUInventoryStockTestBase<CUInventoryEntityFrameworkCoreTestModule>
{
    private sealed record AttemptResult(bool Succeeded, Exception? Error);

    [Fact]
    public async Task Two_Updates_With_The_Same_Stamp_Exactly_One_Succeeds()
    {
        var productId = await SeedProductAsync();
        var persisted = await ProductAppService.GetAsync(productId);

        UpdateProductDto BuildUpdate(string name) => new()
        {
            Name = name,
            Description = persisted.Description,
            Sku = persisted.Sku,
            IsService = persisted.IsService,
            CategoryId = persisted.CategoryId,
            IsActive = persisted.IsActive,
            OrderIndex = persisted.OrderIndex,
            ConcurrencyStamp = persisted.ConcurrencyStamp
        };

        var firstName = $"First-{Guid.NewGuid():N}";
        var secondName = $"Second-{Guid.NewGuid():N}";

        var results = new[]
        {
            await TryInScopeAsync<IProductAppService>(s => s.UpdateAsync(productId, BuildUpdate(firstName))),
            await TryInScopeAsync<IProductAppService>(s => s.UpdateAsync(productId, BuildUpdate(secondName)))
        };

        ShouldHaveExactlyOneWinner(results);

        var final = await ProductAppService.GetAsync(productId);
        final.ShouldSatisfyAllConditions(
            () => final.Name.ShouldBe(firstName),
            () => final.ConcurrencyStamp.ShouldNotBe(persisted.ConcurrencyStamp));
    }

    /// <summary>
    /// The threshold endpoint still uses the caller-supplied stamp, so a stale update must fail rather
    /// than silently win.
    /// </summary>
    [Fact]
    public async Task Two_Threshold_Updates_With_The_Same_Stamp_Exactly_One_Succeeds()
    {
        var warehouseId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        await SeedStockAsync(warehouseId, productId, quantity: 10m);
        var balanceId = await FindBalanceIdAsync(warehouseId, productId);
        var stamp = (await StampOfBalanceAsync(balanceId)).ConcurrencyStamp;

        Task<AttemptResult> TrySetThresholdAsync(decimal threshold) =>
            TryInScopeAsync<IInventoryBalanceAppService>(s => s.SetLowStockThresholdAsync(
                balanceId,
                new SetLowStockThresholdDto { Threshold = threshold, ConcurrencyStamp = stamp }));

        var results = new[]
        {
            await TrySetThresholdAsync(5m),
            await TrySetThresholdAsync(9m)
        };

        ShouldHaveExactlyOneWinner(results);

        var final = await InventoryBalanceAppService.GetAsync(balanceId);
        final.LowStockThreshold.ShouldBe(5m);
    }

    private async Task<AttemptResult> TryInScopeAsync<TService>(Func<TService, Task> action)
        where TService : notnull
    {
        using var scope = ServiceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        try
        {
            await action(service);
            return new AttemptResult(true, null);
        }
        catch (Exception exception)
        {
            return new AttemptResult(false, exception);
        }
    }

    private static void ShouldHaveExactlyOneWinner(AttemptResult[] results)
    {
        results.ShouldSatisfyAllConditions(
            () => results.Count(r => r.Succeeded).ShouldBe(1),
            () => results.Count(r => r.Error is AbpDbConcurrencyException).ShouldBe(1));
    }
}
