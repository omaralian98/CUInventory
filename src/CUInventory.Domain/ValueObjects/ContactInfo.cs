using System;
using System.Collections.Generic;
using CUInventory.Common;
using Volo.Abp.Domain.Values;

namespace CUInventory.ValueObjects;

public class ContactInfo : ValueObject, IEquatable<ContactInfo>
{
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Address Address { get; private set; }

    private ContactInfo()
    {
    }

    public ContactInfo(Email email, PhoneNumber phoneNumber, Address address)
    {
        Email = Guard.NotNull(email, nameof(email));
        PhoneNumber = Guard.NotNull(phoneNumber, nameof(phoneNumber));
        Address = Guard.NotNull(address, nameof(address));
    }

    public bool Equals(ContactInfo? other)
        => other is not null
           && Email == other.Email
           && PhoneNumber == other.PhoneNumber
           && Address == other.Address;

    public override bool Equals(object? obj) => Equals(obj as ContactInfo);

    public override int GetHashCode() => HashCode.Combine(Email, PhoneNumber, Address);

    public override string ToString() => $"{Email}, {PhoneNumber}, {Address}";

    public static bool operator ==(ContactInfo? left, ContactInfo? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(ContactInfo? left, ContactInfo? right) => !(left == right);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Email;
        yield return PhoneNumber;
        yield return Address;
    }
}
