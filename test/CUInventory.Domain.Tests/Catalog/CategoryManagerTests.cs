using System;
using System.Threading.Tasks;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Exceptions;
using CUInventory.Catalog.Managers;
using CUInventory.Catalog.Repositories;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CUInventory.Catalog;

public class CategoryManagerTests
{
    [Fact]
    public async Task CreateAsync_Throws_When_Name_Already_Exists()
    {
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync("Books").Returns(true);
        var manager = new CategoryManager(repository);

        await Should.ThrowAsync<CategoryNameAlreadyExistsDomainException>(
            () => manager.CreateAsync("Books"));
    }

    [Fact]
    public async Task UpdateAsync_Applies_The_New_Name()
    {
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        var manager = new CategoryManager(repository);
        var category = new Category(Guid.NewGuid(), "Old");

        category = await manager.UpdateAsync(category, "New", orderIndex: 5, isActive: true);

        category.Name.ShouldBe("New");
        category.OrderIndex.ShouldBe(5);
        category.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task UpdateAsync_Throws_When_New_Name_Already_Exists()
    {
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync("Taken").Returns(true);
        var manager = new CategoryManager(repository);
        var category = new Category(Guid.NewGuid(), "Old");

        await Should.ThrowAsync<CategoryNameAlreadyExistsDomainException>(
            () => manager.UpdateAsync(category, "Taken", orderIndex: 0, isActive: true));
    }

    [Fact]
    public async Task UpdateAsync_Keeps_The_Name_And_Skips_The_Uniqueness_Check_When_Unchanged()
    {
        var repository = Substitute.For<ICategoryRepository>();
        var manager = new CategoryManager(repository);
        var category = new Category(Guid.NewGuid(), "Same");

        category = await manager.UpdateAsync(category, "Same", orderIndex: 1, isActive: false);

        category.Name.ShouldBe("Same");
        await repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
    }
}
