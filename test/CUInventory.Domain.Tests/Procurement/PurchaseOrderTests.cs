using System;
using System.Linq;
using CUInventory.Procurement.Aggregates;
using CUInventory.Procurement.Events;
using CUInventory.Procurement.Exceptions;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Procurement;

public class PurchaseOrderTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;

    private static PurchaseOrder NewOrder(params (Guid productId, decimal quantity, decimal unitCost)[] lines)
        => new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            lines.Select(l => new PurchaseOrderLineData(Guid.NewGuid(), l.productId, new Quantity(l.quantity), new Money(l.unitCost))).ToArray());

    [Fact]
    public void Confirm_Moves_To_Confirmed_And_Raises_The_Event()
    {
        var order = NewOrder((Guid.NewGuid(), 10m, 5m));

        order.Confirm(Now);

        order.ShouldSatisfyAllConditions(
            () => order.Status.ShouldBe(PurchaseOrderStatus.Confirmed),
            () => order.GetLocalEvents().Select(e => e.EventData).OfType<PurchaseOrderConfirmedDomainEvent>().ShouldHaveSingleItem());
    }

    [Fact]
    public void Ctor_Rejects_Duplicate_Product_Lines()
    {
        var productId = Guid.NewGuid();

        var ex = Should.Throw<PurchaseOrderDuplicateProductLineDomainException>(
            () => NewOrder((productId, 10m, 5m), (productId, 4m, 6m)));
        ex.ShouldSatisfyAllConditions(
            () => ex.Code.ShouldBe(CUInventoryDomainErrorCodes.PurchaseOrderDuplicateProductLine),
            () => ex.Data["ProductId"].ShouldBe(productId));
    }

    [Fact]
    public void Confirm_Throws_When_There_Are_No_Lines()
    {
        var order = NewOrder();

        var ex = Should.Throw<PurchaseOrderHasNoLinesDomainException>(() => order.Confirm(Now));
        ex.Code.ShouldBe(CUInventoryDomainErrorCodes.PurchaseOrderHasNoLines);
    }

    [Fact]
    public void RegisterReceipt_Throws_When_Not_Confirmed()
    {
        var productId = Guid.NewGuid();
        var order = NewOrder((productId, 10m, 5m));

        Should.Throw<PurchaseOrderNotConfirmedDomainException>(() => order.RegisterReceipt(productId, new Quantity(5m)));
    }

    [Fact]
    public void RegisterReceipt_Partial_Sets_PartiallyReceived()
    {
        var productId = Guid.NewGuid();
        var order = NewOrder((productId, 10m, 5m));
        order.Confirm(Now);

        order.RegisterReceipt(productId, new Quantity(4m));

        order.Status.ShouldBe(PurchaseOrderStatus.PartiallyReceived);
    }

    [Fact]
    public void RegisterReceipt_Completing_All_Lines_Sets_FullyReceived()
    {
        var productId = Guid.NewGuid();
        var order = NewOrder((productId, 10m, 5m));
        order.Confirm(Now);

        order.RegisterReceipt(productId, new Quantity(10m));

        order.Status.ShouldBe(PurchaseOrderStatus.FullyReceived);
    }

    [Fact]
    public void RegisterReceipt_Throws_When_Product_Is_Not_On_The_Order()
    {
        var order = NewOrder((Guid.NewGuid(), 10m, 5m));
        order.Confirm(Now);

        Should.Throw<PurchaseOrderLineNotFoundDomainException>(() => order.RegisterReceipt(Guid.NewGuid(), new Quantity(1m)));
    }

    [Fact]
    public void RegisterReceipt_Throws_When_Quantity_Exceeds_Outstanding()
    {
        var productId = Guid.NewGuid();
        var order = NewOrder((productId, 10m, 5m));
        order.Confirm(Now);

        var ex = Should.Throw<PurchaseOrderReceiptExceedsOrderedDomainException>(() => order.RegisterReceipt(productId, new Quantity(11m)));
        ex.ShouldSatisfyAllConditions(
            () => ex.Data["Requested"].ShouldBe(11m),
            () => ex.Data["Outstanding"].ShouldBe(10m));
    }

    [Fact]
    public void Cancel_Is_Allowed_From_Confirmed()
    {
        var order = NewOrder((Guid.NewGuid(), 10m, 5m));
        order.Confirm(Now);

        order.Cancel();

        order.Status.ShouldBe(PurchaseOrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_Throws_After_Stock_Was_Received()
    {
        var productId = Guid.NewGuid();
        var order = NewOrder((productId, 10m, 5m));
        order.Confirm(Now);
        order.RegisterReceipt(productId, new Quantity(4m));

        Should.Throw<PurchaseOrderCannotBeCancelledDomainException>(() => order.Cancel());
    }
}
