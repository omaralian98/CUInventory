using CUInventory.Catalog.ValueObjects;
using CUInventory.ValueObjects;
using Shouldly;
using Xunit;

namespace CUInventory.Catalog;

public class SkuTests
{
    [Fact]
    public void Skus_With_Same_Normalized_Value_Are_Equal()
    {
        var a = new Sku(" abc-123 ");
        var b = new Sku("ABC-123");

        a.ShouldBe(b);
        (a == b).ShouldBeTrue();
        (a != b).ShouldBeFalse();
        a.GetHashCode().ShouldBe(b.GetHashCode());
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
