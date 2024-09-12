using System;
using UnityEngine;

namespace SoRR
{
    public static class GameObjectExtensions
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject is null) throw new ArgumentNullException(nameof(gameObject));
            T? component = gameObject.GetComponent<T>();
            return (bool)component ? component : gameObject.AddComponent<T>();
        }
        public static Component GetOrAddComponent(this GameObject gameObject, Type componentType)
        {
            if (gameObject is null) throw new ArgumentNullException(nameof(gameObject));
            Component? component = gameObject.GetComponent(componentType);
            return (bool)component ? component : gameObject.AddComponent(componentType);
        }

    }
}
