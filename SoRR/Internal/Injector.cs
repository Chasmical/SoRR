using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace SoRR
{
    public static class Injector
    {
        private static readonly Dictionary<Type, StaticInjectionData> typeData = [];
        public static StaticInjectionData GetInjectionData(Type type)
        {
            if (!typeData.TryGetValue(type, out StaticInjectionData? injectionData))
                typeData.Add(type, injectionData = new StaticInjectionData(type));
            return injectionData;
        }

        private static readonly Dictionary<Type, Service> services = [];
        public static Service GetService(Type serviceType)
        {
            if (!services.TryGetValue(serviceType, out Service? service))
            {
                service = (Service)FormatterServices.GetUninitializedObject(serviceType);
                services.Add(serviceType, service);

                try
                {
                    serviceType.GetConstructor([])!.Invoke(service, []);
                    Inject(service);
                    return service;
                }
                catch
                {
                    services.Remove(serviceType);
                    throw;
                }
            }
            return service;
        }

        public static void Inject(object instance)
            => Inject(instance, GetInjectionData(instance.GetType()));
        public static void Inject(object instance, StaticInjectionData injectionData)
        {
            foreach (StaticInjectionInfo injection in injectionData.Injections)
            {
                object? service = ResolveInjection(instance, injection);
                injection.Resolver.Resolve(injection, instance, service);
            }
        }

        private static object? ResolveInjection(object instance, StaticInjectionInfo info)
        {
            if (info.Token.Value is Type type)
            {
                if (type == typeof(GameObject) || typeof(Component).IsAssignableFrom(type))
                {
                    Transform? child = ResolvePath(((Component)instance).transform, info.Path, true);
                    if (child is null) return null;
                    if (type == typeof(GameObject)) return child.gameObject;

                    Component component = child.GetComponent(type);
                    return component ? component : child.gameObject.AddComponent(type);
                }
                if (typeof(Service).IsAssignableFrom(type))
                {
                    return GetService(type);
                }
            }
            throw new NotImplementedException();
        }
        public static Transform? ResolvePath(Transform transform, ReadOnlySpan<string> path, bool createChildren)
        {
            foreach (string name in path)
            {
                Transform? next;
                if (name.Length > 0 && name[0] == '.')
                {
                    if (name.Length > 1 && name[1] == '.')
                    {
                        next = transform.parent;
                        if (!next) return null;
                    }
                    else next = transform;
                }
                else
                {
                    next = transform.Find(name);
                    if (!next)
                    {
                        if (!createChildren) return null;
                        next = new GameObject(name).transform;
                        next.SetParent(transform, false);
                    }
                }
                transform = next;
            }
            return transform;
        }

    }
}
