using System;
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

    }
}
