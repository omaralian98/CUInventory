using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace CUInventory.ValueObjects;

public class Money : ValueObject, IEquatable<Money>
{
    public static Money Zero => new(0m);

    public decimal Amount { get; private set; }

    private Money()
    {
    }

    public Money(decimal amount)
    {
        Check.Positive(amount, nameof(amount));
        
        Amount = amount;
    }

    public Money Add(Money other) => new(Amount + other.Amount);

    public Money Subtract(Money other) => new(Amount - other.Amount);

    public Money Multiply(decimal factor) => new(Amount * factor);

    public bool Equals(Money? other) => other is not null && Amount == other.Amount;

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => Amount.GetHashCode();

    public override string ToString() => Amount.ToString("0.##");

    public static bool operator ==(Money? left, Money? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Money? left, Money? right) => !(left == right);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
    }
}
