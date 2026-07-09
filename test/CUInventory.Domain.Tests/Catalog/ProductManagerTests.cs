using System;
using System.Threading.Tasks;
using CUInventory.Catalog.Aggregates;
using CUInventory.Catalog.Exceptions;
using CUInventory.Catalog.Managers;
using CUInventory.Catalog.Repositories;
using CUInventory.Catalog.ValueObjects;
using CUInventory.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace CUInventory.Catalog;

public class ProductManagerTests
{
    private static Product NewProduct(string name, Sku? sku)
        => new(Guid.NewGuid(), name, description: null, sku, isService: false, categoryId: null);

    [Fact]
    public async Task SetSkuAsync_Throws_When_Sku_Already_Used_By_Another_Product()
    {
        var repository = Substitute.For<IProductRepository>();
        var sku = new Sku("ABC");
        repository.GetProductBySkuOrDefaultAsync(sku).Returns(NewProduct("Existing", sku));
        var manager = new ProductManager(repository);
        var product = NewProduct("Widget", sku: null);

        await Should.ThrowAsync<ProductSkuAlreadyExistsDomainException>(
            () => manager.SetSkuAsync(product, sku));
    }

    [Fact]
    public async Task SetSkuAsync_Assigns_Sku_When_Unique()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.GetProductBySkuOrDefaultAsync(Arg.Any<Sku>()).Returns((Product?)null);
        var manager = new ProductManager(repository);
        var product = NewProduct("Widget", sku: null);

        product = await manager.SetSkuAsync(product, new Sku("ABC"));

        product.SKU.ShouldBe(new Sku("ABC"));
    }

    [Fact]
    public async Task SetSkuAsync_Allows_Clearing_The_Sku_Without_Querying()
    {
        var repository = Substitute.For<IProductRepository>();
        var manager = new ProductManager(repository);
        var product = NewProduct("Widget", new Sku("ABC"));

        product = await manager.SetSkuAsync(product, null);

        product.SKU.ShouldBeNull();
        await repository.DidNotReceive().GetProductBySkuOrDefaultAsync(Arg.Any<Sku>());
    }

    [Fact]
    public async Task SetSkuAsync_Is_A_NoOp_When_Sku_Unchanged()
    {
        var repository = Substitute.For<IProductRepository>();
        var manager = new ProductManager(repository);
        var product = NewProduct("Widget", new Sku("ABC"));

        product = await manager.SetSkuAsync(product, new Sku("ABC"));

        product.SKU.ShouldBe(new Sku("ABC"));
        await repository.DidNotReceive().GetProductBySkuOrDefaultAsync(Arg.Any<Sku>());
    }
}
