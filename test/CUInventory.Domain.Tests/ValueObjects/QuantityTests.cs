using System;
using CUInventory.Common.Exceptions;
using Shouldly;
using Xunit;

namespace CUInventory.ValueObjects;

public class QuantityTests
{
    [Fact]
    public void Rejects_Negative_Values()
    {
        Should.Throw<ArgumentMustBeNonNegativeDomainException>(() => new Quantity(-1m));
    }

    [Fact]
    public void Zero_Is_Zero()
    {
        Quantity.Zero.ShouldSatisfyAllConditions(
            () => Quantity.Zero.Value.ShouldBe(0m),
            () => Quantity.Zero.IsZero.ShouldBeTrue());
    }

    [Fact]
    public void Add_Sums_The_Values()
    {
        new Quantity(10m).Add(new Quantity(5m)).Value.ShouldBe(15m);
    }

    [Fact]
    public void Subtract_Reduces_The_Value()
    {
        new Quantity(10m).Subtract(new Quantity(4m)).Value.ShouldBe(6m);
    }

    [Fact]
    public void Subtract_Throws_When_Result_Is_Negative()
    {
        Should.Throw<ArgumentMustBeNonNegativeDomainException>(() => new Quantity(3m).Subtract(new Quantity(5m)));
    }

    [Fact]
    public void Equal_When_Value_Matches()
    {
        var a = new Quantity(7m);
        var b = new Quantity(7m);

        a.ShouldSatisfyAllConditions(
            () => a.ShouldBe(b),
            () => (a == b).ShouldBeTrue(),
            () => (a != b).ShouldBeFalse(),
            () => a.GetHashCode().ShouldBe(b.GetHashCode()));
    }

    [Fact]
    public void Not_Equal_When_Value_Differs()
    {
        var a = new Quantity(7m);
        var b = new Quantity(8m);

        a.ShouldNotBe(b);
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void ToString_Returns_The_Value()
    {
        new Quantity(7.25m).ToString().ShouldBe("7.25");
    }
}
