using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides a set of static methods for throwing various exceptions.</para>
    /// </summary>
    public static class Guard
    {
        /// <summary>
        ///   <para>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is <see langword="null"/>.</para>
        /// </summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds. If you omit thi parameter, the name of <paramref name="argument"/> is used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null) ThrowNull(paramName);
        }
        /// <summary>
        ///   <para>Throws an <see cref="ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.</para>
        /// </summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="instance">The object whose type's full name should be included in any resulting <see cref="ObjectDisposedException"/>.</param>
        /// <exception cref="ObjectDisposedException"><paramref name="condition"/> is <see langword="true"/>.</exception>
        public static void ThrowIfDisposed([DoesNotReturnIf(true)] bool condition, object instance)
        {
            if (condition) ThrowDisposed(instance);
        }

        [DoesNotReturn] private static void ThrowNull(string? paramName = null)
            => throw new ArgumentNullException(paramName);
        [DoesNotReturn] private static void ThrowDisposed(object? instance)
            => throw new ObjectDisposedException(instance?.GetType().FullName);

        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is equal to <paramref name="other"/>.</exception>
        public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IEquatable<T>
        {
            if (value.Equals(other)) ThrowEqual(value, other, paramName);
        }
        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not equal to <paramref name="other"/>.</exception>
        public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IEquatable<T>
        {
            if (!value.Equals(other)) ThrowNotEqual(value, other, paramName);
        }
        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to <paramref name="other"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as greater than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than or equal to <paramref name="other"/>.</exception>
        public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(other) <= 0) ThrowLessEqual(value, other, paramName);
        }
        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as greater than or equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <paramref name="other"/>.</exception>
        public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(other) < 0) ThrowLess(value, other, paramName);
        }
        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal to <paramref name="other"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as less than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than or equal to <paramref name="other"/>.</exception>
        public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(other) >= 0) ThrowGreaterEqual(value, other, paramName);
        }
        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as less than or equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is greater than <paramref name="other"/>.</exception>
        public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(other) > 0) ThrowGreater(value, other, paramName);
        }

        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as non-zero.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero.</exception>
        public static void ThrowIfZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IEquatable<T>
        {
            if (value.Equals(default)) ThrowZero(value, paramName);
        }
        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(default) < 0) ThrowNegative(value, paramName);
        }
        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as non-negative and non-zero.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative or zero.</exception>
        public static void ThrowIfNegativeOrZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable<T>
        {
            if (value.CompareTo(default) <= 0) ThrowNegativeOrZero(value, paramName);
        }
        /// <summary>
        ///   <para>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not between <paramref name="min"/> and <paramref name="max"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the objects to validate.</typeparam>
        /// <param name="value">The argument to validate as non-negative and non-zero.</param>
        /// <param name="min">The minimum value to compare with <paramref name="value"/>.</param>
        /// <param name="max">The maximum value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <paramref name="min"/> or greater than <paramref name="max"/>.</exception>
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
