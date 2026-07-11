using System;
using System.Collections.Generic;
using CUInventory.Common;
using Volo.Abp.Domain.Values;

namespace CUInventory.Catalog.ValueObjects;

public class Sku : ValueObject, IEquatable<Sku>
{
    public string Value { get; private set; }

    private Sku()
    {
    }

    public Sku(string value)
    {
        Guard.NotNullOrWhiteSpace(value, nameof(value));
        Value = value.Trim().ToUpperInvariant();
    }

    public bool Equals(Sku? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as Sku);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static bool operator ==(Sku? left, Sku? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Sku? left, Sku? right) => !(left == right);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
