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
    private static CategoryManager CreateManager(ICategoryRepository repository)
        => new CategoryManager(repository).WithTestGuidGenerator();

    [Fact]
    public async Task CreateAsync_Builds_The_Category_When_Name_Is_Free()
    {
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        var manager = CreateManager(repository);

        var category = await manager.CreateAsync("Books");

        category.ShouldSatisfyAllConditions(
            () => category.Name.ShouldBe("Books"),
            () => category.Id.ShouldNotBe(Guid.Empty));
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Name_Already_Exists()
    {
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync("Books").Returns(true);
        var manager = CreateManager(repository);

        var ex = await Should.ThrowAsync<CategoryNameAlreadyExistsDomainException>(
            () => manager.CreateAsync("Books"));

        ex.ShouldSatisfyAllConditions(
            () => ex.Code.ShouldBe(CUInventoryDomainErrorCodes.CategoryNameAlreadyExists),
            () => ex.Data["Name"].ShouldBe("Books"));
    }

    [Theory]
    [InlineData("Old", "New", true)]
    [InlineData("Same", "Same", false)]
    public async Task UpdateAsync_Checks_Uniqueness_Only_When_The_Name_Changes(
        string original, string updated, bool shouldCheck)
    {
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        var manager = CreateManager(repository);
        var category = new Category(Guid.NewGuid(), original);

        category = await manager.UpdateAsync(category, updated, orderIndex: 3, isActive: true);

        category.ShouldSatisfyAllConditions(
            () => category.Name.ShouldBe(updated),
            () => category.OrderIndex.ShouldBe(3),
            () => category.IsActive.ShouldBeTrue());

        if (shouldCheck)
        {
            await repository.Received(1).ExistsAsync(updated);
        }
        else
        {
            await repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        }
    }

    [Fact]
    public async Task UpdateAsync_Throws_When_New_Name_Already_Exists()
    {
        var repository = Substitute.For<ICategoryRepository>();
        repository.ExistsAsync("Taken").Returns(true);
        var manager = CreateManager(repository);
        var category = new Category(Guid.NewGuid(), "Old");

        await Should.ThrowAsync<CategoryNameAlreadyExistsDomainException>(
            () => manager.UpdateAsync(category, "Taken", orderIndex: 0, isActive: true));
    }
}
