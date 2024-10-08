using System;
using UnityEngine;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides a set of extension methods for the <see cref="GameObject"/> class.</para>
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        ///   <para>Gets or adds a component of type <typeparamref name="T"/> on the specified <paramref name="gameObject"/>.</para>
        /// </summary>
        /// <typeparam name="T">The type of the component to get or add.</typeparam>
        /// <param name="gameObject">The game object to get or add the component of type <typeparamref name="T"/> on.</param>
        /// <returns>The component of type <typeparamref name="T"/> on the specified <paramref name="gameObject"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject is null) throw new ArgumentNullException(nameof(gameObject));
            return gameObject.TryGetComponent(out T component) ? component : gameObject.AddComponent<T>();
        }
        /// <summary>
        ///   <para>Gets or adds a component of the specified <paramref name="componentType"/> on the specified <paramref name="gameObject"/>.</para>
        /// </summary>
        /// <param name="gameObject">The game object to get or add the component of the specified <paramref name="componentType"/> on.</param>
        /// <param name="componentType">The type of the component to get or add.</param>
        /// <returns>The component of the specified <paramref name="componentType"/> on the specified <paramref name="gameObject"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> is <see langword="null"/>.</exception>
        public static Component GetOrAddComponent(this GameObject gameObject, Type componentType)
        {
            if (gameObject is null) throw new ArgumentNullException(nameof(gameObject));
            return gameObject.TryGetComponent(componentType, out Component component) ? component : gameObject.AddComponent(componentType);
        }

    }
}
