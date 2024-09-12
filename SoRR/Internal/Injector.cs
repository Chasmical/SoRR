using System;
using UnityEngine;

namespace SoRR
{
    public static class Injector
    {
        public static void Inject(object instance)
            => Inject(instance, InjectionData.Get(instance.GetType()));
        private static void Inject(object instance, InjectionData injectionData)
        {
            foreach (InjectionInfo injection in injectionData.Injections)
            {
                object? service = Resolve(instance, injection.TypeToken, injection.Path);
                injection.Resolver.Resolve(injection, instance, service);
            }
        }

        private static object? Resolve(object instance, Type type, ReadOnlySpan<string> path)
        {
            if (type.IsSubclassOf(typeof(Component)))
            {
                Transform? tr = ResolvePath(((Component)instance).transform, path, true);
                return tr?.gameObject.GetOrAddComponent(type);
            }
            throw new InvalidOperationException();
        }

        private static Transform? ResolvePath(Transform transform, ReadOnlySpan<string> path, bool createChildren)
        {
            for (int i = 0; i < path.Length; i++)
            {
                string name = path[i];
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
