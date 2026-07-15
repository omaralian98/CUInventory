using System;
using System.Linq;
using System.Threading.Tasks;
using CUInventory.Catalog.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Catalog;

public abstract class ProductAppServiceTests<TStartupModule> : CUInventoryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IProductAppService _productAppService;
    private readonly ICategoryAppService _categoryAppService;

    protected ProductAppServiceTests()
    {
        _productAppService = GetRequiredService<IProductAppService>();
        _categoryAppService = GetRequiredService<ICategoryAppService>();
    }

    [Fact]
    public async Task Should_Create_Get_Update_And_Delete_A_Product()
    {
        var name = $"Widget-{Guid.NewGuid():N}";
        var sku = $"SKU-{Guid.NewGuid():N}";

        var created = await _productAppService.CreateAsync(new CreateProductDto
        {
            Name = name,
            Description = "A widget",
            Sku = sku,
            IsService = false,
            OrderIndex = 3
        });

        created.ShouldSatisfyAllConditions(
            () => created.Id.ShouldNotBe(Guid.Empty),
            () => created.Name.ShouldBe(name),
            () => created.Description.ShouldBe("A widget"),
            // The Sku value object normalizes to upper-invariant.
            () => created.Sku.ShouldBe(sku.ToUpperInvariant()),
            () => created.IsActive.ShouldBeTrue(),
            () => created.OrderIndex.ShouldBe(3));

        var fetched = await _productAppService.GetAsync(created.Id);
        fetched.Name.ShouldBe(name);

        var updated = await _productAppService.UpdateAsync(created.Id, new UpdateProductDto
        {
            Name = $"{name}-v2",
            IsService = true,
            IsActive = false,
            OrderIndex = 7,
            ConcurrencyStamp = fetched.ConcurrencyStamp
        });

        updated.ShouldSatisfyAllConditions(
            () => updated.Name.ShouldBe($"{name}-v2"),
            () => updated.IsService.ShouldBeTrue(),
            () => updated.IsActive.ShouldBeFalse(),
            () => updated.Sku.ShouldBeNull(),
            () => updated.OrderIndex.ShouldBe(7));

        await _productAppService.DeleteAsync(created.Id);
        await Should.ThrowAsync<EntityNotFoundException>(() => _productAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task Should_Reject_A_Duplicate_Sku()
    {
        var sku = $"DUP-{Guid.NewGuid():N}";
        await _productAppService.CreateAsync(new CreateProductDto { Name = $"First-{Guid.NewGuid():N}", Sku = sku });

        await Should.ThrowAsync<BusinessException>(
            () => _productAppService.CreateAsync(new CreateProductDto { Name = $"Second-{Guid.NewGuid():N}", Sku = sku }));
    }

    [Fact]
    public async Task Should_Filter_By_Name_Sku_And_Category()
    {
        var category = await _categoryAppService.CreateAsync(new CreateCategoryDto { Name = $"Cat-{Guid.NewGuid():N}" });
        var categoryId = category.Id;
        var token = Guid.NewGuid().ToString("N");

        var inCategory = await _productAppService.CreateAsync(
            new CreateProductDto { Name = $"{token}-InCat", Sku = $"{token}-A", CategoryId = categoryId });
        await _productAppService.CreateAsync(
            new CreateProductDto { Name = $"{token}-NoCat", Sku = $"{token}-B" });

        var byName = await _productAppService.GetListAsync(new GetProductListDto { Filter = $"{token}-InCat" });
        byName.Items.ShouldHaveSingleItem().Id.ShouldBe(inCategory.Id);

        var bySku = await _productAppService.GetListAsync(new GetProductListDto { Filter = $"{token}-A".ToUpperInvariant() });
        bySku.Items.ShouldHaveSingleItem().Id.ShouldBe(inCategory.Id);

        var byCategory = await _productAppService.GetListAsync(new GetProductListDto { Filter = token, CategoryId = categoryId });
        byCategory.Items.ShouldHaveSingleItem().Id.ShouldBe(inCategory.Id);
    }

    [Fact]
    public async Task Should_Exclude_Inactive_Products_Unless_IncludeInactive_Is_Set()
    {
        var name = $"Retired-{Guid.NewGuid():N}";
        var created = await _productAppService.CreateAsync(new CreateProductDto { Name = name });
        var fetched = await _productAppService.GetAsync(created.Id);

        await _productAppService.UpdateAsync(created.Id, new UpdateProductDto
        {
            Name = name,
            IsActive = false,
            ConcurrencyStamp = fetched.ConcurrencyStamp
        });

        var activeOnly = await _productAppService.GetListAsync(new GetProductListDto { Filter = name });
        activeOnly.TotalCount.ShouldBe(0);

        var includingInactive = await _productAppService.GetListAsync(
            new GetProductListDto { Filter = name, IncludeInactive = true });
        includingInactive.Items.ShouldContain(p => p.Id == created.Id);
    }

    [Fact]
    public async Task Should_Order_By_OrderIndex_When_No_Sorting_Given()
    {
        var token = Guid.NewGuid().ToString("N");
        await _productAppService.CreateAsync(new CreateProductDto { Name = $"{token}-c", OrderIndex = 2 });
        await _productAppService.CreateAsync(new CreateProductDto { Name = $"{token}-a", OrderIndex = 0 });
        await _productAppService.CreateAsync(new CreateProductDto { Name = $"{token}-b", OrderIndex = 1 });

        var list = await _productAppService.GetListAsync(new GetProductListDto { Filter = token });

        list.Items.Select(p => p.OrderIndex).ShouldBe(new[] { 0, 1, 2 });
    }
}
