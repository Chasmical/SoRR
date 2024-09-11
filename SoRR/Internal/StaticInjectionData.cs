using System;
using System.Collections.Generic;
using System.Reflection;
using Chasm.Collections;

namespace SoRR
{
    public sealed class StaticInjectionData
    {
        public Type Type { get; }

        private readonly InjectionToken[] tokens;
        public ReadOnlySpan<InjectionToken> Tokens => tokens;
        private readonly StaticInjectionInfo[] injections;
        public ReadOnlySpan<StaticInjectionInfo> Injections => injections;

        internal StaticInjectionData(Type type)
        {
            Type = type;
            tokens = ResolveTokens(type);
            injections = ResolveInjections(type);
        }

        private static readonly Type?[] PrimitiveTypes =
        [
            null,
            typeof(object),
            typeof(ValueType),
            typeof(Enum),
            typeof(Delegate),
            typeof(MulticastDelegate),
            typeof(UnityEngine.Component),
            typeof(UnityEngine.Object),
        ];

        private static InjectionToken[] ResolveTokens(Type type)
        {
            List<InjectionToken> list = [];

            Type? next = type;
            while (Array.IndexOf(PrimitiveTypes, next) == -1)
            {
                list.Add(new InjectionToken(next!));
                next = next!.BaseType;
            }

            foreach (Type interfaceType in type.GetInterfaces())
                list.Add(new InjectionToken(interfaceType));

            return list.ToArray();
        }
        private static StaticInjectionInfo[] ResolveInjections(Type type)
        {
            List<StaticInjectionInfo> list = [];
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            Type? baseType = type.BaseType;
            if (Array.IndexOf(PrimitiveTypes, baseType) == -1)
            {
                StaticInjectionData baseData = Injector.GetInjectionData(baseType!);
                list.AddRange(baseData.injections);
            }

            foreach (FieldInfo field in type.GetFields(flags))
            {
                InjectAttribute? attr = field.GetCustomAttribute<InjectAttribute>();
                if (attr is null) continue;
                InjectionToken token = attr.Token ?? field.FieldType;
                IInjectionResolver resolver = new FieldInjectionResolver(field);
                list.Add(new StaticInjectionInfo(token, resolver, attr));
            }
            foreach (PropertyInfo property in type.GetProperties(flags))
            {
                InjectAttribute? attr = property.GetCustomAttribute<InjectAttribute>();
                if (attr is null) continue;
                InjectionToken token = attr.Token ?? property.PropertyType;
                IInjectionResolver resolver = new PropertyInjectionResolver(property);
                list.Add(new StaticInjectionInfo(token, resolver, attr));
            }

            return list.ToArray();
        }

    }
    public sealed class StaticInjectionInfo
    {
        public InjectionToken Token { get; }
        public IInjectionResolver Resolver { get; }

        private readonly string[] pathParts;
        public ReadOnlySpan<string> Path => pathParts;

        internal StaticInjectionInfo(InjectionToken token, IInjectionResolver resolver, InjectAttribute attribute)
        {
            Token = token;
            Resolver = resolver;
            pathParts = attribute.Path?.Split('/') ?? [];
        }

    }
}
