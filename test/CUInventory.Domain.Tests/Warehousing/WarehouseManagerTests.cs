using System;
using System.Threading.Tasks;
using CUInventory.ValueObjects;
using CUInventory.Warehousing.Aggregates;
using CUInventory.Warehousing.Exceptions;
using CUInventory.Warehousing.Managers;
using CUInventory.Warehousing.Repositories;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Shouldly;
using Xunit;

namespace CUInventory.Warehousing;

public class WarehouseManagerTests
{
    private static Address NewAddress() => new("Damascus", "Damascus", "Al-Mazzeh St");

    private static Warehouse NewWarehouse(string name = "Main", string code = "WH-1")
        => new(Guid.NewGuid(), name, code, NewAddress());

    private static IWarehouseRepository FreeRepository()
    {
        var repository = Substitute.For<IWarehouseRepository>();
        repository.GetByCodeOrDefaultAsync(Arg.Any<string>()).ReturnsNull();
        return repository;
    }

    private static WarehouseManager CreateManager(IWarehouseRepository repository)
        => new WarehouseManager(repository).WithTestGuidGenerator();

    [Fact]
    public async Task CreateAsync_Builds_An_Active_Warehouse_With_Normalized_Code()
    {
        var repository = FreeRepository();
        var manager = CreateManager(repository);

        var warehouse = await manager.CreateAsync("Main", "wh-1", NewAddress());

        warehouse.ShouldSatisfyAllConditions(
            () => warehouse.Name.ShouldBe("Main"),
            () => warehouse.Code.ShouldBe("WH-1"),
            () => warehouse.IsActive.ShouldBeTrue(),
            () => warehouse.Id.ShouldNotBe(Guid.Empty));
    }

    [Fact]
    public async Task CreateAsync_Throws_When_Code_Belongs_To_Another_Warehouse()
    {
        var repository = FreeRepository();
        repository.GetByCodeOrDefaultAsync("WH-1").Returns(NewWarehouse(code: "WH-1"));
        var manager = CreateManager(repository);

        var ex = await Should.ThrowAsync<WarehouseCodeAlreadyExistsDomainException>(
            () => manager.CreateAsync("Other", "wh-1", NewAddress()));

        ex.ShouldSatisfyAllConditions(
            () => ex.Code.ShouldBe(CUInventoryDomainErrorCodes.WarehouseCodeAlreadyExists),
            () => ex.Data["Code"].ShouldBe("WH-1"));
    }

    [Fact]
    public async Task UpdateAsync_Skips_The_Uniqueness_Check_When_Code_Unchanged()
    {
        var repository = FreeRepository();
        var manager = CreateManager(repository);
        var warehouse = NewWarehouse(code: "WH-1");

        await manager.UpdateAsync(warehouse, "Renamed", "wh-1", NewAddress(), isActive: true);

        warehouse.Name.ShouldBe("Renamed");
        await repository.DidNotReceive().GetByCodeOrDefaultAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task UpdateAsync_Rechecks_Uniqueness_When_Code_Changes()
    {
        var repository = FreeRepository();
        var manager = CreateManager(repository);
        var warehouse = NewWarehouse(code: "WH-1");

        await manager.UpdateAsync(warehouse, "Main", "WH-2", NewAddress(), isActive: false);

        warehouse.ShouldSatisfyAllConditions(
            () => warehouse.Code.ShouldBe("WH-2"),
            () => warehouse.IsActive.ShouldBeFalse());
        await repository.Received(1).GetByCodeOrDefaultAsync("WH-2");
    }
}
