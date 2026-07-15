using System;
using System.Threading.Tasks;
using CUInventory.Shared.Dtos;
using CUInventory.Warehousing.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Warehousing;

public abstract class WarehouseAppServiceTests<TStartupModule> : CUInventoryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IWarehouseAppService _warehouseAppService;

    protected WarehouseAppServiceTests()
    {
        _warehouseAppService = GetRequiredService<IWarehouseAppService>();
    }

    private static AddressDto Address() =>
        new() { Governorate = "Damascus", City = "Damascus", Street = "Main St" };

    [Fact]
    public async Task Should_Create_Get_Update_And_Delete_A_Warehouse()
    {
        var token = Guid.NewGuid().ToString("N")[..12];
        var name = $"Central-{token}";

        var created = await _warehouseAppService.CreateAsync(new CreateWarehouseDto
        {
            Name = name,
            // Lower-case code should be normalized to upper-invariant by the domain.
            Code = $"wh-{token}",
            Address = Address(),
            OrderIndex = 2
        });

        created.ShouldSatisfyAllConditions(
            () => created.Id.ShouldNotBe(Guid.Empty),
            () => created.Name.ShouldBe(name),
            () => created.Code.ShouldBe($"WH-{token}".ToUpperInvariant()),
            () => created.IsActive.ShouldBeTrue(),
            () => created.OrderIndex.ShouldBe(2));

        var fetched = await _warehouseAppService.GetAsync(created.Id);

        var updated = await _warehouseAppService.UpdateAsync(created.Id, new UpdateWarehouseDto
        {
            Name = $"{name}-v2",
            Code = $"wh-{token}",
            Address = Address(),
            IsActive = false,
            OrderIndex = 9,
            ConcurrencyStamp = fetched.ConcurrencyStamp
        });

        updated.ShouldSatisfyAllConditions(
            () => updated.Name.ShouldBe($"{name}-v2"),
            () => updated.IsActive.ShouldBeFalse(),
            () => updated.OrderIndex.ShouldBe(9));

        await _warehouseAppService.DeleteAsync(created.Id);
        await Should.ThrowAsync<EntityNotFoundException>(() => _warehouseAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task Should_Reject_A_Duplicate_Code()
    {
        var code = $"CODE-{Guid.NewGuid():N}"[..16];
        await _warehouseAppService.CreateAsync(new CreateWarehouseDto
        {
            Name = $"First-{Guid.NewGuid():N}", Code = code, Address = Address()
        });

        await Should.ThrowAsync<BusinessException>(
            () => _warehouseAppService.CreateAsync(new CreateWarehouseDto
            {
                // Same code in a different case must still collide because codes are normalized.
                Name = $"Second-{Guid.NewGuid():N}", Code = code.ToLowerInvariant(), Address = Address()
            }));
    }

    [Fact]
    public async Task Should_Exclude_Inactive_Warehouses_Unless_IncludeInactive_Is_Set()
    {
        var token = Guid.NewGuid().ToString("N")[..12];
        var created = await _warehouseAppService.CreateAsync(new CreateWarehouseDto
        {
            Name = $"Closing-{token}", Code = $"cl-{token}", Address = Address()
        });
        var fetched = await _warehouseAppService.GetAsync(created.Id);

        await _warehouseAppService.UpdateAsync(created.Id, new UpdateWarehouseDto
        {
            Name = $"Closing-{token}",
            Code = $"cl-{token}",
            Address = Address(),
            IsActive = false,
            ConcurrencyStamp = fetched.ConcurrencyStamp
        });

        var activeOnly = await _warehouseAppService.GetListAsync(new GetWarehouseListDto { Filter = token });
        activeOnly.TotalCount.ShouldBe(0);

        var includingInactive = await _warehouseAppService.GetListAsync(
            new GetWarehouseListDto { Filter = token, IncludeInactive = true });
        includingInactive.Items.ShouldContain(w => w.Id == created.Id);
    }
}
