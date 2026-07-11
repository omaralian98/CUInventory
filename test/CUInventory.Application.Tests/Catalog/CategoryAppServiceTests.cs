using System;
using System.Threading.Tasks;
using CUInventory.Catalog;
using CUInventory.Catalog.Dtos;
using Shouldly;
using Volo.Abp;
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
            created.Id, new UpdateCategoryDto { Name = updatedName, OrderIndex = 5, IsActive = false });
        updated.Name.ShouldBe(updatedName);
        updated.IsActive.ShouldBeFalse();
        updated.OrderIndex.ShouldBe(5);

        var list = await _categoryAppService.GetListAsync(new GetCategoryListDto { Filter = updatedName });
        list.TotalCount.ShouldBe(1);
        list.Items.ShouldContain(c => c.Id == created.Id);

        await _categoryAppService.DeleteAsync(created.Id);
        await Should.ThrowAsync<EntityNotFoundException>(() => _categoryAppService.GetAsync(created.Id));
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
