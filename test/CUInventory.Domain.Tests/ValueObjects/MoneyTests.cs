using System;
using CUInventory.Common.Exceptions;
using Shouldly;
using Xunit;

namespace CUInventory.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Rejects_Negative_Amounts()
    {
        Should.Throw<ArgumentMustBeNonNegativeDomainException>(() => new Money(-1m));
    }

    [Fact]
    public void Zero_Has_A_Zero_Amount()
    {
        Money.Zero.Amount.ShouldBe(0m);
    }

    [Fact]
    public void Add_Sums_The_Amounts()
    {
        new Money(10m).Add(new Money(5m)).Amount.ShouldBe(15m);
    }

    [Fact]
    public void Subtract_Reduces_The_Amount()
    {
        new Money(10m).Subtract(new Money(4m)).Amount.ShouldBe(6m);
    }

    [Fact]
    public void Subtract_Throws_When_Result_Is_Negative()
    {
        Should.Throw<ArgumentMustBeNonNegativeDomainException>(() => new Money(3m).Subtract(new Money(5m)));
    }

    [Fact]
    public void Multiply_Scales_The_Amount()
    {
        new Money(4m).Multiply(3m).Amount.ShouldBe(12m);
    }

    [Fact]
    public void Equal_When_Amount_Matches()
    {
        var a = new Money(12.5m);
        var b = new Money(12.5m);

        a.ShouldSatisfyAllConditions(
            () => a.ShouldBe(b),
            () => (a == b).ShouldBeTrue(),
            () => (a != b).ShouldBeFalse(),
            () => a.GetHashCode().ShouldBe(b.GetHashCode()));
    }

    [Fact]
    public void Not_Equal_When_Amount_Differs()
    {
        var a = new Money(12.5m);
        var b = new Money(9m);

        a.ShouldNotBe(b);
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void ToString_Returns_The_Amount()
    {
        new Money(12.5m).ToString().ShouldBe("12.5");
    }
}
