using System;
using System.Linq;
using CUInventory.Inventory;
using CUInventory.Sales.Aggregates;
using CUInventory.Sales.Events;
using CUInventory.Sales.Exceptions;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Sales;

public class SaleTests
{
    private static readonly DateTime Now = DomainServiceTestExtensions.TestNow;
    private static readonly Guid Product = Guid.NewGuid();
    private static readonly Guid Warehouse = Guid.NewGuid();

    private static SaleLineData Line(decimal quantity = 4m)
        => new(Guid.NewGuid(), Product, new Quantity(quantity), new Money(20m), AllocationStrategyKind.Fifo, Warehouse, null, null);

    private static Sale NewSale() => new(Guid.NewGuid(), [Line()]);

    [Fact]
    public void Constructor_Creates_Draft_Sale_With_Lines()
    {
        var lineId = Guid.NewGuid();
        var sale = new Sale(Guid.NewGuid(), [new SaleLineData(lineId, Product, new Quantity(4m), new Money(20m), AllocationStrategyKind.Fifo, Warehouse, null, null)]);

        sale.ShouldSatisfyAllConditions(
            () => sale.Status.ShouldBe(SaleStatus.Draft),
            () => sale.ConfirmedAt.ShouldBeNull(),
            () => sale.Lines.ShouldHaveSingleItem(),
            () => sale.Lines.Single().Id.ShouldBe(lineId),
            () => sale.Lines.Single().ProductId.ShouldBe(Product));
    }

    [Fact]
    public void Confirm_Sets_Confirmed_Status_ConfirmedAt_And_Raises_SaleCompleted()
    {
        var sale = NewSale();

        sale.Confirm(Now);

        sale.ShouldSatisfyAllConditions(
            () => sale.Status.ShouldBe(SaleStatus.Confirmed),
            () => sale.ConfirmedAt.ShouldBe(Now),
            () => sale.GetLocalEvents().Select(e => e.EventData).OfType<SaleCompletedDomainEvent>().ShouldHaveSingleItem());
    }

    [Fact]
    public void Confirm_When_Not_Draft_Throws()
    {
        var sale = NewSale();
        sale.Confirm(Now);

        Should.Throw<SaleNotInDraftStateDomainException>(() => sale.Confirm(Now));
    }

    [Fact]
    public void Confirm_With_No_Lines_Throws()
    {
        var sale = new Sale(Guid.NewGuid(), []);

        Should.Throw<SaleHasNoLinesDomainException>(() => sale.Confirm(Now));
    }

    [Fact]
    public void Cancel_Sets_Cancelled_Status()
    {
        var sale = NewSale();

        sale.Cancel();

        sale.Status.ShouldBe(SaleStatus.Cancelled);
    }

    [Fact]
    public void Cancel_When_Not_Draft_Throws()
    {
        var sale = NewSale();
        sale.Confirm(Now);

        Should.Throw<SaleNotInDraftStateDomainException>(() => sale.Cancel());
    }
}
