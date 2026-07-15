using System;
using System.Threading.Tasks;
using CUInventory.Procurement.Dtos;
using CUInventory.Shared.Dtos;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CUInventory.Procurement;

public abstract class PurchaseOrderAppServiceTests<TStartupModule> : CUInventoryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IPurchaseOrderAppService _purchaseOrderAppService;

    protected PurchaseOrderAppServiceTests()
    {
        _purchaseOrderAppService = GetRequiredService<IPurchaseOrderAppService>();
    }

    [Fact]
    public async Task Should_Create_Confirm_And_Cancel_PurchaseOrder()
    {
        var created = await _purchaseOrderAppService.CreateAsync(new CreatePurchaseOrderDto
        {
            SupplierId = Guid.NewGuid(),
            DestinationWarehouseId = Guid.NewGuid(),
            Lines =
            {
                new CreatePurchaseOrderLineDto { ProductId = Guid.NewGuid(), OrderedQuantity = 10m, UnitCost = 5m }
            }
        });

        created.Status.ShouldBe(PurchaseOrderStatus.Draft);
        created.Lines.Count.ShouldBe(1);
        created.Lines[0].OrderedQuantity.ShouldBe(10m);
        created.Lines[0].UnitCost.ShouldBe(5m);
        created.Lines[0].OutstandingQuantity.ShouldBe(10m);

        var confirmed = await _purchaseOrderAppService.ConfirmAsync(
            created.Id, new ConcurrencyStampDto { ConcurrencyStamp = created.ConcurrencyStamp });
        confirmed.Status.ShouldBe(PurchaseOrderStatus.Confirmed);

        var cancelled = await _purchaseOrderAppService.CancelAsync(
            created.Id, new ConcurrencyStampDto { ConcurrencyStamp = confirmed.ConcurrencyStamp });
        cancelled.Status.ShouldBe(PurchaseOrderStatus.Cancelled);
    }

    [Fact]
    public async Task Should_Reject_Confirm_With_Stale_ConcurrencyStamp()
    {
        var created = await _purchaseOrderAppService.CreateAsync(new CreatePurchaseOrderDto
        {
            SupplierId = Guid.NewGuid(),
            DestinationWarehouseId = Guid.NewGuid(),
            Lines =
            {
                new CreatePurchaseOrderLineDto { ProductId = Guid.NewGuid(), OrderedQuantity = 10m, UnitCost = 5m }
            }
        });

        var staleStamp = created.ConcurrencyStamp;

        await _purchaseOrderAppService.ConfirmAsync(
            created.Id, new ConcurrencyStampDto { ConcurrencyStamp = staleStamp });

        await Should.ThrowAsync<AbpDbConcurrencyException>(
            () => _purchaseOrderAppService.CancelAsync(
                created.Id, new ConcurrencyStampDto { ConcurrencyStamp = staleStamp }));
    }

    [Fact]
    public async Task Should_Delete_A_Draft_PurchaseOrder()
    {
        var created = await _purchaseOrderAppService.CreateAsync(NewOrder(Guid.NewGuid(), Guid.NewGuid()));

        await _purchaseOrderAppService.DeleteAsync(created.Id);

        await Should.ThrowAsync<EntityNotFoundException>(() => _purchaseOrderAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task Confirming_Twice_Throws()
    {
        var created = await _purchaseOrderAppService.CreateAsync(NewOrder(Guid.NewGuid(), Guid.NewGuid()));
        var confirmed = await _purchaseOrderAppService.ConfirmAsync(
            created.Id, new ConcurrencyStampDto { ConcurrencyStamp = created.ConcurrencyStamp });

        await Should.ThrowAsync<BusinessException>(
            () => _purchaseOrderAppService.ConfirmAsync(
                created.Id, new ConcurrencyStampDto { ConcurrencyStamp = confirmed.ConcurrencyStamp }));
    }

    private static CreatePurchaseOrderDto NewOrder(Guid supplierId, Guid warehouseId) => new()
    {
        SupplierId = supplierId,
        DestinationWarehouseId = warehouseId,
        Lines = { new CreatePurchaseOrderLineDto { ProductId = Guid.NewGuid(), OrderedQuantity = 10m, UnitCost = 5m } }
    };
}
