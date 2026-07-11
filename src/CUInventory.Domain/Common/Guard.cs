using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CUInventory.Common.Exceptions;
using CUInventory.ValueObjects;

namespace CUInventory.Common;

public static class Guard
{
    public static T NotNull<T>([NotNull] T? value, string parameterName)
    {
        if (value is null)
        {
            throw new RequiredArgumentDomainException(parameterName);
        }

        return value;
    }

    public static string NotNullOrWhiteSpace([NotNull] string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new RequiredArgumentDomainException(parameterName);
        }

        return value;
    }

    public static void NotEmpty<T>([NotNull] IReadOnlyCollection<T>? value, string parameterName)
    {
        if (value is null || value.Count == 0)
        {
            throw new RequiredArgumentDomainException(parameterName);
        }
    }

    public static void Positive(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentMustBePositiveDomainException(parameterName, value);
        }
    }

    public static void Positive(Quantity quantity, string parameterName)
    {
        if (quantity.IsZero)
        {
            throw new ArgumentMustBePositiveDomainException(parameterName, quantity.Value);
        }
    }

    public static void NonNegative(decimal value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentMustBeNonNegativeDomainException(parameterName, value);
        }
    }

    public static void NonNegative(decimal? value, string parameterName)
    {
        if (value is < 0)
        {
            throw new ArgumentMustBeNonNegativeDomainException(parameterName, value.Value);
        }
    }

    public static void MatchesPattern(string value, Regex pattern, string parameterName)
    {
        if (!pattern.IsMatch(value))
        {
            throw new ArgumentInvalidFormatDomainException(parameterName, value);
        }
    }
}
