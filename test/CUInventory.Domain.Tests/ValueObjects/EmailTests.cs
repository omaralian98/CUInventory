using System;
using Shouldly;
using Xunit;

namespace CUInventory.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com", "user@example.com")]
    [InlineData("  User@Example.COM ", "user@example.com")]
    [InlineData("MiXeD@CaSe.Org", "mixed@case.org")]
    public void Normalizes_By_Trimming_And_Lowercasing(string input, string expected)
    {
        new Email(input).Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@no-local.com")]
    [InlineData("has space@email.com")]
    [InlineData("two@@at.com")]
    public void Rejects_Invalid_Values(string input)
    {
        Should.Throw<ArgumentException>(() => new Email(input));
    }

    [Theory]
    [InlineData(" A@B.com ", "a@b.com")]
    [InlineData("a@b.com", "A@B.COM")]
    public void Equal_When_Normalized_Value_Matches(string left, string right)
    {
        var a = new Email(left);
        var b = new Email(right);

        a.ShouldSatisfyAllConditions(
            () => a.ShouldBe(b),
            () => (a == b).ShouldBeTrue(),
            () => (a != b).ShouldBeFalse(),
            () => a.GetHashCode().ShouldBe(b.GetHashCode()));
    }

    [Fact]
    public void Not_Equal_When_Value_Differs()
    {
        var a = new Email("a@b.com");
        var b = new Email("x@y.com");

        a.ShouldNotBe(b);
        (a != b).ShouldBeTrue();
    }

    [Fact]
    public void ToString_Returns_Normalized_Value()
    {
        new Email(" A@B.com ").ToString().ShouldBe("a@b.com");
    }
}
