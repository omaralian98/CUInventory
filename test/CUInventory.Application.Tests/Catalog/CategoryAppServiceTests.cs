using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Catalog;
using CUInventory.Catalog.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Catalog;

public abstract class CategoryAppServiceTests<TStartupModule> : CUInventoryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ICategoryAppService _categoryAppService;

    protected CategoryAppServiceTests()
    {
        _categoryAppService = GetRequiredService<ICategoryAppService>();
    }

    [Fact]
    public async Task Should_Create_Get_Update_List_And_Delete_Category()
    {
        var initialName = $"Electronics-{Guid.NewGuid():N}";
        var updatedName = $"Gadgets-{Guid.NewGuid():N}";

        var created = await _categoryAppService.CreateAsync(
            new CreateCategoryDto { Name = initialName, OrderIndex = 1, IsActive = true });

        created.Id.ShouldNotBe(Guid.Empty);
        created.Name.ShouldBe(initialName);
        created.IsActive.ShouldBeTrue();
        created.OrderIndex.ShouldBe(1);

        var fetched = await _categoryAppService.GetAsync(created.Id);
        fetched.Name.ShouldBe(initialName);

        var updated = await _categoryAppService.UpdateAsync(
            created.Id,
            new UpdateCategoryDto
            {
                Name = updatedName, OrderIndex = 5, IsActive = false, ConcurrencyStamp = fetched.ConcurrencyStamp
            });
        updated.Name.ShouldBe(updatedName);
        updated.IsActive.ShouldBeFalse();
        updated.OrderIndex.ShouldBe(5);

        // The category was deactivated above, so the default (active-only) list excludes it,
        // and IncludeInactive brings it back.
        var activeOnly = await _categoryAppService.GetListAsync(new GetCategoryListDto { Filter = updatedName });
        activeOnly.TotalCount.ShouldBe(0);

        var list = await _categoryAppService.GetListAsync(
            new GetCategoryListDto { Filter = updatedName, IncludeInactive = true });
        list.TotalCount.ShouldBe(1);
        list.Items.ShouldContain(c => c.Id == created.Id);

        await _categoryAppService.DeleteAsync(created.Id);
        await Should.ThrowAsync<EntityNotFoundException>(() => _categoryAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task Should_Get_Update_And_Delete_An_Already_Inactive_Category()
    {
        var name = $"Deactivated-{Guid.NewGuid():N}";

        var created = await _categoryAppService.CreateAsync(
            new CreateCategoryDto { Name = name, OrderIndex = 1, IsActive = true });

        await _categoryAppService.UpdateAsync(
            created.Id,
            new UpdateCategoryDto
            {
                Name = name, OrderIndex = 1, IsActive = false, ConcurrencyStamp = created.ConcurrencyStamp
            });

        var fetched = await _categoryAppService.GetAsync(created.Id);
        fetched.Id.ShouldBe(created.Id);
        fetched.IsActive.ShouldBeFalse();

        var reactivated = await _categoryAppService.UpdateAsync(
            created.Id,
            new UpdateCategoryDto
            {
                Name = name, OrderIndex = 1, IsActive = true, ConcurrencyStamp = fetched.ConcurrencyStamp
            });
        reactivated.IsActive.ShouldBeTrue();

        await _categoryAppService.UpdateAsync(
            created.Id,
            new UpdateCategoryDto
            {
                Name = name, OrderIndex = 1, IsActive = false, ConcurrencyStamp = reactivated.ConcurrencyStamp
            });

        await _categoryAppService.DeleteAsync(created.Id);
        await Should.ThrowAsync<EntityNotFoundException>(() => _categoryAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task Should_Return_List_Ordered_By_OrderIndex_When_No_Sorting_Given()
    {
        var token = Guid.NewGuid().ToString("N");

        await _categoryAppService.CreateAsync(new CreateCategoryDto { Name = $"{token}-c", OrderIndex = 2 });
        await _categoryAppService.CreateAsync(new CreateCategoryDto { Name = $"{token}-a", OrderIndex = 0 });
        await _categoryAppService.CreateAsync(new CreateCategoryDto { Name = $"{token}-b", OrderIndex = 1 });

        var list = await _categoryAppService.GetListAsync(new GetCategoryListDto { Filter = token });

        list.Items.Count.ShouldBe(3);
        list.Items.Select(c => c.OrderIndex).ShouldBe(new[] { 0, 1, 2 });
    }

    [Fact]
    public async Task Should_Reject_Update_With_Stale_ConcurrencyStamp()
    {
        var name = $"Concurrent-{Guid.NewGuid():N}";

        var created = await _categoryAppService.CreateAsync(new CreateCategoryDto { Name = name, OrderIndex = 1 });
        var staleStamp = created.ConcurrencyStamp;

        await _categoryAppService.UpdateAsync(
            created.Id,
            new UpdateCategoryDto { Name = name, OrderIndex = 2, ConcurrencyStamp = staleStamp });

        await Should.ThrowAsync<AbpDbConcurrencyException>(
            () => _categoryAppService.UpdateAsync(
                created.Id,
                new UpdateCategoryDto { Name = name, OrderIndex = 3, ConcurrencyStamp = staleStamp }));
    }

    [Fact]
    public async Task Should_Reject_Update_With_Empty_ConcurrencyStamp()
    {
        var name = $"NoStamp-{Guid.NewGuid():N}";

        var created = await _categoryAppService.CreateAsync(new CreateCategoryDto { Name = name, OrderIndex = 1 });

        await Should.ThrowAsync<AbpDbConcurrencyException>(
            () => _categoryAppService.UpdateAsync(
                created.Id, new UpdateCategoryDto { Name = name, OrderIndex = 2 }));
    }

    [Fact]
    public async Task Should_Reject_Duplicate_Category_Name()
    {
        var name = $"Unique-{Guid.NewGuid():N}";
        await _categoryAppService.CreateAsync(new CreateCategoryDto { Name = name });

        await Should.ThrowAsync<BusinessException>(
            () => _categoryAppService.CreateAsync(new CreateCategoryDto { Name = name }));
    }
}
