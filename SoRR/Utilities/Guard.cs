using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SoRR
{
    public static class Guard
    {
        public static void ThrowIfNull([NotNull] object? obj, [CallerArgumentExpression(nameof(obj))] string? paramName = null)
        {
            if (obj is null) ThrowNull(paramName);
        }
        public static void ThrowIfDisposed([DoesNotReturnIf(true)] bool isDisposed, object instance)
        {
            if (isDisposed) ThrowDisposed(instance);
        }

        [DoesNotReturn] private static void ThrowNull(string? paramName = null)
            => throw new ArgumentNullException(paramName);
        [DoesNotReturn] private static void ThrowDisposed(object? instance)
            => throw new ObjectDisposedException(instance?.GetType().FullName);

        public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IEquatable<T>
        {
            if (value.Equals(other)) ThrowEqual(value, other, paramName);
        }
        public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IEquatable<T>
        {
            if (!value.Equals(other)) ThrowNotEqual(value, other, paramName);
        }
        public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(other) <= 0) ThrowLessEqual(value, other, paramName);
        }
        public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(other) < 0) ThrowLess(value, other, paramName);
        }
        public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(other) >= 0) ThrowGreaterEqual(value, other, paramName);
        }
        public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(other) > 0) ThrowGreater(value, other, paramName);
        }

        public static void ThrowIfZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IEquatable<T>
        {
            if (value.Equals(default)) ThrowZero(value, paramName);
        }
        public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(default) < 0) ThrowNegative(value, paramName);
        }
        public static void ThrowIfNegativeOrZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(default) <= 0) ThrowNegativeOrZero(value, paramName);
        }
        public static void ThrowIfNotInRange<T>(T value, T min, T max, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0) ThrowNotInRange(value, min, max, paramName);
        }

        [DoesNotReturn] private static void ThrowEqual<T>(T value, T other, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must not be equal to '{other}'.");
        [DoesNotReturn] private static void ThrowNotEqual<T>(T value, T other, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be equal to '{other}'.");
        [DoesNotReturn] private static void ThrowLessEqual<T>(T value, T other, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be greater than '{other}'.");
        [DoesNotReturn] private static void ThrowLess<T>(T value, T other, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be greater than or equal to '{other}'.");
        [DoesNotReturn] private static void ThrowGreaterEqual<T>(T value, T other, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be less than '{other}'.");
        [DoesNotReturn] private static void ThrowGreater<T>(T value, T other, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be less than or equal to '{other}'.");

        [DoesNotReturn] private static void ThrowZero<T>(T value, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be a non-zero value.");
        [DoesNotReturn] private static void ThrowNegative<T>(T value, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be a non-negative value.");
        [DoesNotReturn] private static void ThrowNegativeOrZero<T>(T value, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be a non-negative and non-zero value.");
        [DoesNotReturn] private static void ThrowNotInRange<T>(T value, T min, T max, string? paramName)
            => throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be between '{min}' and '{max}'.");

    }
}
