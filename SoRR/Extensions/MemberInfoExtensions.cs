using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides a set of extension methods for retrieving custom attributes of .</para>
    /// </summary>
    public static class MemberInfoExtensions
    {
        /// <summary>
        ///   <para>Retrieves a custom attribute of the specified type that is applied to the specified assembly, and returns a value indicating whether such an attribute is found.</para>
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="element">The assembly to inspect.</param>
        /// <param name="attribute">When this method returns, contains the custom attribute that matches <typeparamref name="T"/>, or <see langword="null"/> if no such attribute is found.</param>
        /// <returns><see langword="true"/>, if an attribute that matches <typeparamref name="T"/> is found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/>.</exception>
        [Pure] public static bool TryGetCustomAttribute<T>(this Assembly element, [NotNullWhen(true)] out T? attribute) where T : Attribute
            => TryGetCustomAttributeCore((T[])element.GetCustomAttributes<T>(), out attribute);
        /// <summary>
        ///   <para>Retrieves a custom attribute of the specified type that is applied to the specified module, and returns a value indicating whether such an attribute is found.</para>
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="element">The module to inspect.</param>
        /// <param name="attribute">When this method returns, contains the custom attribute that matches <typeparamref name="T"/>, or <see langword="null"/> if no such attribute is found.</param>
        /// <returns><see langword="true"/>, if an attribute that matches <typeparamref name="T"/> is found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/>.</exception>
        [Pure] public static bool TryGetCustomAttribute<T>(this Module element, [NotNullWhen(true)] out T? attribute) where T : Attribute
            => TryGetCustomAttributeCore((T[])element.GetCustomAttributes<T>(), out attribute);
        /// <summary>
        ///   <para>Retrieves a custom attribute of the specified type that is applied to the specified member, and returns a value indicating whether such an attribute is found.</para>
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="element">The member to inspect.</param>
        /// <param name="attribute">When this method returns, contains the custom attribute that matches <typeparamref name="T"/>, or <see langword="null"/> if no such attribute is found.</param>
        /// <returns><see langword="true"/>, if an attribute that matches <typeparamref name="T"/> is found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/>.</exception>
        [Pure] public static bool TryGetCustomAttribute<T>(this MemberInfo element, [NotNullWhen(true)] out T? attribute) where T : Attribute
            => TryGetCustomAttributeCore((T[])element.GetCustomAttributes<T>(), out attribute);
        /// <summary>
        ///   <para>Retrieves a custom attribute of the specified type that is applied to the specified parameter, and returns a value indicating whether such an attribute is found.</para>
        /// </summary>
        /// <typeparam name="T">The type of attribute to search for.</typeparam>
        /// <param name="element">The parameter to inspect.</param>
        /// <param name="attribute">When this method returns, contains the custom attribute that matches <typeparamref name="T"/>, or <see langword="null"/> if no such attribute is found.</param>
        /// <returns><see langword="true"/>, if an attribute that matches <typeparamref name="T"/> is found; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/>.</exception>
        [Pure] public static bool TryGetCustomAttribute<T>(this ParameterInfo element, [NotNullWhen(true)] out T? attribute) where T : Attribute
            => TryGetCustomAttributeCore((T[])element.GetCustomAttributes<T>(), out attribute);

        [Pure] private static bool TryGetCustomAttributeCore<T>(T[] attributes, [NotNullWhen(true)] out T? attribute) where T : Attribute
        {
            if (attributes.Length > 0)
            {
                attribute = attributes[0];
                return true;
            }
            attribute = default;
            return false;
        }

        /// <summary>
        ///   <para>Returns the nullability of the specified <paramref name="field"/>.</para>
        /// </summary>
        /// <param name="field">The field to determine the nullability of.</param>
        /// <returns>The nullability of the specified <paramref name="field"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
        [Pure] public static Nullability GetNullability(this FieldInfo field)
        {
            Guard.ThrowIfNull(field);
            return GetNullabilityCore(field.FieldType, field.DeclaringType, field.GetCustomAttributesData());
        }
        /// <summary>
        ///   <para>Returns the nullability of the specified <paramref name="property"/>.</para>
        /// </summary>
        /// <param name="property">The property to determine the nullability of.</param>
        /// <returns>The nullability of the specified <paramref name="property"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
        [Pure] public static Nullability GetNullability(this PropertyInfo property)
        {
            Guard.ThrowIfNull(property);
            return GetNullabilityCore(property.PropertyType, property.DeclaringType, property.GetCustomAttributesData());
        }
        /// <summary>
        ///   <para>Returns the nullability of the specified <paramref name="parameter"/>.</para>
        /// </summary>
        /// <param name="parameter">The parameter to determine the nullability of.</param>
        /// <returns>The nullability of the specified <paramref name="parameter"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="parameter"/> is <see langword="null"/>.</exception>
        [Pure] public static Nullability GetNullability(this ParameterInfo parameter)
        {
            Guard.ThrowIfNull(parameter);
            return GetNullabilityCore(parameter.ParameterType, parameter.Member, parameter.GetCustomAttributesData());
        }

        [Pure] private static Nullability GetNullabilityCore(Type type, MemberInfo? parent, IList<CustomAttributeData> attributes)
        {
            if (type.IsByRef || type.IsPointer)
                type = type.GetElementType()!;

            if (type.IsValueType)
                return Nullable.GetUnderlyingType(type) is not null ? Nullability.Nullable : Nullability.NotNull;

            object? value = GetNullabilityFirstAttributeValue(attributes, "NullableAttribute");
            Nullability result = ParseNullableState(value);

            if (result == Nullability.Unknown)
            {
                while (parent is not null)
                {
                    value = GetNullabilityFirstAttributeValue(parent.GetCustomAttributesData(), "NullableContextAttribute");
                    if (value is byte b)
                    {
                        result = (Nullability)b;
                        break;
                    }
                    parent = parent.DeclaringType;
                }
            }
            return result;
        }
        [Pure] private static object? GetNullabilityFirstAttributeValue(IList<CustomAttributeData> attributes, string attrName)
        {
            const string attrNamespace = "System.Runtime.CompilerServices";

            foreach (CustomAttributeData attr in attributes)
            {
                Type type = attr.AttributeType;
                if (type.Name == attrName && type.Namespace == attrNamespace)
                {
                    IList<CustomAttributeTypedArgument> ctorArgs = attr.ConstructorArguments;
                    if (ctorArgs.Count == 1) return ctorArgs[0].Value;
                }
            }
            return null;
        }
        [Pure] private static Nullability ParseNullableState(object? state)
        {
            switch (state)
            {
                case byte b:
                    return (Nullability)b;
                case ReadOnlyCollection<CustomAttributeTypedArgument> args
                    when args.Count > 0 && args[0].Value is byte b:
                    return (Nullability)b;
                default:
                    return Nullability.Unknown;
            }
        }

    }
    /// <summary>
    ///   <para>Defines nullability modes of fields, properties and parameters.</para>
    /// </summary>
    public enum Nullability
    {
        /// <summary>
        ///   <para>Specifies that the value's nullability is unknown.</para>
        /// </summary>
        Unknown = 0,
        /// <summary>
        ///   <para>Specifies that the value is not null.</para>
        /// </summary>
        NotNull = 1,
        /// <summary>
        ///   <para>Specifies that the value may be null.</para>
        /// </summary>
        Nullable = 2,
    }
}
