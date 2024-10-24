﻿using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace SoRR
{
    public sealed class InjectionData
    {
        public Type Type { get; }
        private readonly InjectionInfo[] injections;
        public ReadOnlySpan<InjectionInfo> Injections => injections;

        private InjectionData(Type type)
        {
            Type = type;
            injections = ResolveInjections(type);
        }

        [Pure] private static InjectionInfo[] ResolveInjections(Type type)
        {
            List<InjectionInfo> list = [];
            const BindingFlags anyFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (FieldInfo field in type.GetFields(anyFlags))
                if (field.TryGetCustomAttribute(out InjectAttribute? attr))
                {
                    IInjectionResolver resolver = new FieldInjectionResolver(field);
                    bool isOptional = attr.Optional ?? field.GetNullability() == Nullability.Nullable;
                    list.Add(new InjectionInfo(field.FieldType, attr.Path, resolver, isOptional));
                }

            foreach (PropertyInfo property in type.GetProperties(anyFlags))
                if (property.TryGetCustomAttribute(out InjectAttribute? attr))
                {
                    IInjectionResolver resolver = new PropertyInjectionResolver(property);
                    bool isOptional = attr.Optional ?? property.GetNullability() == Nullability.Nullable;
                    list.Add(new InjectionInfo(property.PropertyType, attr.Path, resolver, isOptional));
                }

            return list.ToArray();
        }

        private static readonly Dictionary<Type, InjectionData> dict = [];
        [Pure] public static InjectionData Get(Type type)
        {
            if (!dict.TryGetValue(type, out InjectionData? injectionData))
                dict.Add(type, injectionData = new InjectionData(type));
            return injectionData;
        }

    }
    public readonly struct InjectionInfo
    {
        public Type TypeToken { get; }
        private readonly string[] pathParts;
        public ReadOnlySpan<string> Path => pathParts;
        public IInjectionResolver Resolver { get; }
        public bool IsOptional { get; }

        public InjectionInfo(Type typeToken, string? path, IInjectionResolver resolver, bool isOptional)
        {
            TypeToken = typeToken;
            pathParts = path?.Split('/') ?? [];
            Resolver = resolver;
            IsOptional = isOptional;
        }
    }
}
