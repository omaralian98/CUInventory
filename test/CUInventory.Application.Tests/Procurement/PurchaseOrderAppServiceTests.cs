using System;
using System.Threading.Tasks;
using CUInventory.Procurement.Dtos;
using Shouldly;
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

        var confirmed = await _purchaseOrderAppService.ConfirmAsync(created.Id);
        confirmed.Status.ShouldBe(PurchaseOrderStatus.Confirmed);

        var cancelled = await _purchaseOrderAppService.CancelAsync(created.Id);
        cancelled.Status.ShouldBe(PurchaseOrderStatus.Cancelled);
    }
}
