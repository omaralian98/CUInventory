using System;
using Shouldly;
using Xunit;

namespace CUInventory.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("+963 11 2345678")]
    [InlineData("011-234-5678")]
    [InlineData("123.456.7890")]
    [InlineData("1234567")]
    public void Accepts_Valid_Values_And_Trims(string input)
    {
        new PhoneNumber($"  {input}  ").Value.ShouldBe(input);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("12345")]
    [InlineData("phone")]
    [InlineData("+12a45678")]
    public void Rejects_Invalid_Values(string input)
    {
        Should.Throw<ArgumentException>(() => new PhoneNumber(input));
    }

    [Fact]
    public void Equal_When_Value_Matches()
    {
        var a = new PhoneNumber("+963 11 2345678");
        var b = new PhoneNumber(" +963 11 2345678 ");

        a.ShouldSatisfyAllConditions(
            () => a.ShouldBe(b),
            () => (a == b).ShouldBeTrue(),
            () => a.GetHashCode().ShouldBe(b.GetHashCode()));
    }

    [Fact]
    public void Not_Equal_When_Value_Differs()
    {
        new PhoneNumber("+963 11 2345678").ShouldNotBe(new PhoneNumber("+963 11 9999999"));
    }
}
