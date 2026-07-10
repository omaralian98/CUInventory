using System;
using System.Threading.Tasks;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Exceptions;
using CUInventory.Catalog.Managers;
using CUInventory.Catalog.Repositories;
using CUInventory.Catalog.ValueObjects;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Shouldly;
using Xunit;

namespace CUInventory.Catalog;

public class ProductManagerTests
{
    private static Product NewProduct(string name, Sku? sku)
        => new(Guid.NewGuid(), name, description: null, sku, isService: false, categoryId: null);

    private static ProductManager CreateManager(IProductRepository repository)
        => new ProductManager(repository).WithTestGuidGenerator();

    [Fact]
    public async Task CreateAsync_Builds_The_Product_And_Validates_The_Sku()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetProductBySkuOrDefaultAsync(Arg.Any<Sku>()).ReturnsNull();
        var manager = CreateManager(repository);

        var product = await manager.CreateAsync("Widget", "desc", new Sku("abc"));

        product.ShouldSatisfyAllConditions(
            () => product.Name.ShouldBe("Widget"),
            () => product.SKU!.Value.ShouldBe("ABC"),
            () => product.Id.ShouldNotBe(Guid.Empty));
    }

    [Fact]
    public async Task SetSkuAsync_Throws_When_Sku_Already_Used_By_Another_Product()
    {
        var repository = Substitute.For<IProductRepository>();
        var sku = new Sku("ABC");
        repository.GetProductBySkuOrDefaultAsync(sku).Returns(NewProduct("Existing", sku));
        var manager = new ProductManager(repository);
        var product = NewProduct("Widget", sku: null);

        var ex = await Should.ThrowAsync<ProductSkuAlreadyExistsDomainException>(
            () => manager.SetSkuAsync(product, sku));

        ex.ShouldSatisfyAllConditions(
            () => ex.Code.ShouldBe(CUInventoryDomainErrorCodes.ProductSkuAlreadyExists),
            () => ex.Data["Sku"].ShouldBe("ABC"));
    }

    [Theory]
    [InlineData("abc", "ABC")]
    [InlineData("xy-1", "XY-1")]
    public async Task SetSkuAsync_Assigns_The_Sku_When_Unique(string input, string expected)
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetProductBySkuOrDefaultAsync(Arg.Any<Sku>()).ReturnsNull();
        var manager = new ProductManager(repository);
        var product = NewProduct("Widget", sku: null);

        product = await manager.SetSkuAsync(product, new Sku(input));

        product.SKU!.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("ABC", "ABC")]
    [InlineData("ABC", null)]
    public async Task SetSkuAsync_Does_Not_Query_When_Nothing_Needs_Checking(string initial, string? target)
    {
        var repository = Substitute.For<IProductRepository>();
        var manager = new ProductManager(repository);
        var product = NewProduct("Widget", new Sku(initial));
        var newSku = target is null ? null : new Sku(target);

        product = await manager.SetSkuAsync(product, newSku);

        product.SKU.ShouldBe(newSku);
        await repository.DidNotReceive().GetProductBySkuOrDefaultAsync(Arg.Any<Sku>());
    }
}
