using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;

namespace SoRR
{
    public static class MemberInfoExtensions
    {
        [Pure] public static bool TryGetCustomAttribute<T>(this Assembly element, [NotNullWhen(true)] out T? attribute) where T : Attribute
            => (attribute = element.GetCustomAttribute<T>()) is not null;
        [Pure] public static bool TryGetCustomAttribute<T>(this Module element, [NotNullWhen(true)] out T? attribute) where T : Attribute
            => (attribute = element.GetCustomAttribute<T>()) is not null;
        [Pure] public static bool TryGetCustomAttribute<T>(this MemberInfo element, [NotNullWhen(true)] out T? attribute) where T : Attribute
            => (attribute = element.GetCustomAttribute<T>()) is not null;
        [Pure] public static bool TryGetCustomAttribute<T>(this ParameterInfo element, [NotNullWhen(true)] out T? attribute) where T : Attribute
            => (attribute = element.GetCustomAttribute<T>()) is not null;

    }
}
