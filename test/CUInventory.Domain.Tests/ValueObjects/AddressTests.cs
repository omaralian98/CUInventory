using System;
using CUInventory.Common.Exceptions;
using Shouldly;
using Xunit;

namespace CUInventory.ValueObjects;

public class AddressTests
{
    public static TheoryData<string, string, string> BlankParts => new()
    {
        { "", "Damascus", "Al-Mazzeh St" },
        { "   ", "Damascus", "Al-Mazzeh St" },
        { "Damascus", "", "Al-Mazzeh St" },
        { "Damascus", "Damascus", "" },
    };

    [Theory]
    [MemberData(nameof(BlankParts))]
    public void Requires_Every_Part(string governorate, string city, string street)
    {
        Should.Throw<RequiredArgumentDomainException>(() => new Address(governorate, city, street));
    }

    [Fact]
    public void Trims_Each_Part()
    {
        var address = new Address(" Damascus ", " Homs ", " Main St ");

        address.ShouldSatisfyAllConditions(
            () => address.Governorate.ShouldBe("Damascus"),
            () => address.City.ShouldBe("Homs"),
            () => address.Street.ShouldBe("Main St"));
    }

    [Fact]
    public void Equal_When_All_Parts_Match()
    {
        var a = new Address("Damascus", "Damascus", "Al-Mazzeh St");
        var b = new Address("Damascus", "Damascus", "Al-Mazzeh St");

        a.ShouldSatisfyAllConditions(
            () => a.ShouldBe(b),
            () => (a == b).ShouldBeTrue(),
            () => a.GetHashCode().ShouldBe(b.GetHashCode()));
    }

    [Theory]
    [InlineData("Aleppo", "Damascus", "Al-Mazzeh St")]
    [InlineData("Damascus", "Homs", "Al-Mazzeh St")]
    [InlineData("Damascus", "Damascus", "Other St")]
    public void Not_Equal_When_Any_Part_Differs(string governorate, string city, string street)
    {
        var baseline = new Address("Damascus", "Damascus", "Al-Mazzeh St");
        var other = new Address(governorate, city, street);

        baseline.ShouldNotBe(other);
        (baseline != other).ShouldBeTrue();
    }
}
