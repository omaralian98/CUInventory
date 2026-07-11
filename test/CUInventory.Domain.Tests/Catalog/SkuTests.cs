using System;
using CUInventory.Catalog.ValueObjects;
using CUInventory.Common.Exceptions;
using Shouldly;
using Xunit;

namespace CUInventory.Catalog;

public class SkuTests
{
    [Theory]
    [InlineData(" abc-123 ", "ABC-123")]
    [InlineData("abc", "ABC")]
    [InlineData("Xyz-9", "XYZ-9")]
    public void Normalizes_By_Trimming_And_Uppercasing(string input, string expected)
    {
        new Sku(input).Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_Blank_Values(string input)
    {
        Should.Throw<RequiredArgumentDomainException>(() => new Sku(input));
    }

    [Theory]
    [InlineData(" abc-123 ", "ABC-123")]
    [InlineData("sku", "SKU")]
    public void Skus_With_Same_Normalized_Value_Are_Equal(string left, string right)
    {
        var a = new Sku(left);
        var b = new Sku(right);

        a.ShouldSatisfyAllConditions(
            () => a.ShouldBe(b),
            () => (a == b).ShouldBeTrue(),
            () => (a != b).ShouldBeFalse(),
            () => a.GetHashCode().ShouldBe(b.GetHashCode()));
    }

    [Fact]
    public void Skus_With_Different_Values_Are_Not_Equal()
    {
        var a = new Sku("ABC");
        var b = new Sku("XYZ");

        a.ShouldNotBe(b);
        (a == b).ShouldBeFalse();
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void Equality_Operators_Handle_Null()
    {
        Sku? a = null;
        Sku? b = null;
        var c = new Sku("ABC");

        (a == b).ShouldBeTrue();
        (a == c).ShouldBeFalse();
        (c == a).ShouldBeFalse();
        (a != c).ShouldBeTrue();
    }
}
