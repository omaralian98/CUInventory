using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace CUInventory.ValueObjects;

public class Address : ValueObject, IEquatable<Address>
{
    public string Governorate { get; private set; }
    public string City { get; private set; }
    public string Street { get; private set; }

    private Address()
    {
    }

    public Address(string governorate, string city, string street)
    {
        Check.NotNullOrWhiteSpace(governorate, nameof(governorate));
        Check.NotNullOrWhiteSpace(city, nameof(city));
        Check.NotNullOrWhiteSpace(street, nameof(street));

        Governorate = governorate.Trim();
        City = city.Trim();
        Street = street.Trim();
    }

    public bool Equals(Address? other)
        => other is not null
           && Governorate == other.Governorate
           && City == other.City
           && Street == other.Street;

    public override bool Equals(object? obj) => Equals(obj as Address);

    public override int GetHashCode() => HashCode.Combine(Governorate, City, Street);

    public override string ToString() => $"{Street}, {City}, {Governorate}";

    public static bool operator ==(Address? left, Address? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Address? left, Address? right) => !(left == right);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Governorate;
        yield return City;
        yield return Street;
    }
}
