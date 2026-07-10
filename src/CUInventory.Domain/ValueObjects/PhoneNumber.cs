using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace CUInventory.ValueObjects;

public partial class PhoneNumber : ValueObject, IEquatable<PhoneNumber>
{
    private static readonly Regex Pattern = MyRegex();

    public string Value { get; private set; }

    private PhoneNumber()
    {
    }

    public PhoneNumber(string value)
    {
        Check.NotNullOrWhiteSpace(value, nameof(value));

        var normalized = value.Trim();
        if (!Pattern.IsMatch(normalized))
        {
            throw new ArgumentException($"'{value}' is not a valid phone number.", nameof(value));
        }

        Value = normalized;
    }

    public bool Equals(PhoneNumber? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as PhoneNumber);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(PhoneNumber? left, PhoneNumber? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(PhoneNumber? left, PhoneNumber? right) => !(left == right);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^\+?[0-9 .-]{7,}$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
