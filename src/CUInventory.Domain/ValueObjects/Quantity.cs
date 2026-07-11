using System;
using System.Collections.Generic;
using CUInventory.Common;
using Volo.Abp.Domain.Values;

namespace CUInventory.ValueObjects;

public class Quantity : ValueObject, IEquatable<Quantity>
{
    public static Quantity Zero => new(0m);

    public decimal Value { get; private set; }

    private Quantity()
    {
    }

    public Quantity(decimal value)
    {
        Guard.NonNegative(value, nameof(value));

        Value = value;
    }

    public Quantity Add(Quantity other) => new(Value + other.Value);

    public Quantity Subtract(Quantity other) => new(Value - other.Value);

    public bool IsZero => Value == 0m;

    public bool Equals(Quantity? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as Quantity);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString("0.##");

    public static bool operator ==(Quantity? left, Quantity? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Quantity? left, Quantity? right) => !(left == right);

    public static bool operator <(Quantity left, Quantity right) => left.Value < right.Value;

    public static bool operator >(Quantity left, Quantity right) => left.Value > right.Value;

    public static bool operator <=(Quantity left, Quantity right) => left.Value <= right.Value;

    public static bool operator >=(Quantity left, Quantity right) => left.Value >= right.Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
