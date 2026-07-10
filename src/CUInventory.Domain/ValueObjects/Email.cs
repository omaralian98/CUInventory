using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace CUInventory.ValueObjects;

public partial class Email : ValueObject, IEquatable<Email>
{
    private static readonly Regex Pattern = MyRegex();

    public string Value { get; private set; }

    private Email()
    {
    }

    public Email(string value)
    {
        Check.NotNullOrWhiteSpace(value, nameof(value));

        var normalized = value.Trim().ToLowerInvariant();
        if (!Pattern.IsMatch(normalized))
        {
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));
        }

        Value = normalized;
    }

    public bool Equals(Email? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as Email);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(Email? left, Email? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Email? left, Email? right) => !(left == right);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
