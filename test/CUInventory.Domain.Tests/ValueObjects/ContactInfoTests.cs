using System;
using Shouldly;
using Xunit;

namespace CUInventory.ValueObjects;

public class ContactInfoTests
{
    private static Email Email() => new("a@b.com");
    private static PhoneNumber Phone() => new("+963 933 405 934");
    private static Address Addr() => new("Damascus", "Damascus", "Al-Mazzeh St");

    private static ContactInfo Baseline() => new(Email(), Phone(), Addr());

    [Fact]
    public void Requires_Every_Component()
    {
        Should.Throw<ArgumentException>(() => new ContactInfo(null!, Phone(), Addr()));
        Should.Throw<ArgumentException>(() => new ContactInfo(Email(), null!, Addr()));
        Should.Throw<ArgumentException>(() => new ContactInfo(Email(), Phone(), null!));
    }

    [Fact]
    public void Equal_When_All_Components_Match()
    {
        var a = Baseline();
        var b = Baseline();

        a.ShouldSatisfyAllConditions(
            () => a.ShouldBe(b),
            () => (a == b).ShouldBeTrue(),
            () => (a != b).ShouldBeFalse(),
            () => a.GetHashCode().ShouldBe(b.GetHashCode()));
    }

    [Theory]
    [InlineData("x@y.com", "+963 933 405 934", "Damascus", "Damascus", "Al-Mazzeh St")]
    [InlineData("a@b.com", "+963 946 149 745", "Damascus", "Damascus", "Al-Mazzeh St")]
    [InlineData("a@b.com", "+963 933 405 934", "Aleppo", "Damascus", "Al-Mazzeh St")]
    [InlineData("a@b.com", "+963 933 405 934", "Damascus", "Homs", "Al-Mazzeh St")]
    [InlineData("a@b.com", "+963 933 405 934", "Damascus", "Damascus", "Other St")]
    public void Not_Equal_When_Any_Component_Differs(
        string email, string phone, string governorate, string city, string street)
    {
        var other = new ContactInfo(
            new Email(email), new PhoneNumber(phone), new Address(governorate, city, street));

        Baseline().ShouldNotBe(other);
        (Baseline() != other).ShouldBeTrue();
    }
}
