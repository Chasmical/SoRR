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
                object? service = Resolve(instance, injection.TypeToken, injection.Path, injection.IsOptional);
                injection.Resolver.Resolve(injection, instance, service);
            }
        }

        private static object? Resolve(object instance, Type type, ReadOnlySpan<string> path, bool isOptional)
        {
            bool isGameObject = type == typeof(GameObject);

            // Resolve GameObject and Component dependencies
            if (isGameObject || type.IsSubclassOf(typeof(Component)))
            {
                // Resolve the Transform component of the target GameObject
                Transform? tr = ((Component)instance).transform;
                if (!path.IsEmpty)
                {
                    tr = ResolvePath(tr, path, !isOptional);
                    if (tr is null) return null;
                }

                // If it's just the GameObject that's needed, return it
                GameObject go = tr.gameObject;
                if (isGameObject) return go;

                // Try to return an already existing matching component
                if (go.TryGetComponent(type, out Component? component))
                    return component;

                // If the component is required, attempt to create it
                if (!isOptional)
                {
                    if (type.IsInterface || type.IsAbstract) throw new InvalidOperationException();
                    component = go.AddComponent(type);
                }

                return component;
            }

            throw new NotSupportedException($"Type {type} cannot be injected as a dependency.");
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
