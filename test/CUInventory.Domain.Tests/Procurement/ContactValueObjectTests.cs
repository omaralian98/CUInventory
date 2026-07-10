using System;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Procurement;

public class ContactValueObjectTests
{
    [Theory]
    [InlineData("  User@Example.COM ", "user@example.com")]
    [InlineData("a@b.co", "a@b.co")]
    public void Email_Is_Normalized(string input, string expected)
    {
        new Email(input).Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@domain")]
    [InlineData("@no-local.com")]
    [InlineData("")]
    public void Email_Rejects_Invalid_Values(string input)
    {
        Should.Throw<Exception>(() => new Email(input));
    }

    [Fact]
    public void Emails_With_Same_Normalized_Value_Are_Equal()
    {
        var a = new Email(" A@B.com ");
        var b = new Email("a@b.com");

        a.ShouldBe(b);
        (a == b).ShouldBeTrue();
        a.GetHashCode().ShouldBe(b.GetHashCode());
    }

    [Theory]
    [InlineData("+963 11 2345678")]
    [InlineData("011-234-5678")]
    public void PhoneNumber_Accepts_Valid_Values(string input)
    {
        new PhoneNumber(input).Value.ShouldBe(input.Trim());
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("phone")]
    [InlineData("")]
    public void PhoneNumber_Rejects_Invalid_Values(string input)
    {
        Should.Throw<Exception>(() => new PhoneNumber(input));
    }

    [Fact]
    public void Address_Requires_All_Parts()
    {
        Should.Throw<Exception>(() => new Address("", "Damascus", "St"));
        Should.Throw<Exception>(() => new Address("Damascus", "", "St"));
        Should.Throw<Exception>(() => new Address("Damascus", "Damascus", ""));
    }

    [Fact]
    public void Addresses_With_Same_Parts_Are_Equal()
    {
        var a = new Address("Damascus", "Damascus", "Al-Mazzeh St");
        var b = new Address("Damascus", "Damascus", "Al-Mazzeh St");

        a.ShouldBe(b);
        (a == b).ShouldBeTrue();
    }

    [Fact]
    public void ContactInfo_Equality_Is_Value_Based()
    {
        var a = new ContactInfo(new Email("a@b.com"), new PhoneNumber("+963 11 2345678"),
            new Address("Damascus", "Damascus", "St"));
        var b = new ContactInfo(new Email("a@b.com"), new PhoneNumber("+963 11 2345678"),
            new Address("Damascus", "Damascus", "St"));
        var c = new ContactInfo(new Email("x@y.com"), new PhoneNumber("+963 11 2345678"),
            new Address("Damascus", "Damascus", "St"));

        (a == b).ShouldBeTrue();
        (a != c).ShouldBeTrue();
    }
}
