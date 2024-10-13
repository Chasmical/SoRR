using System;
using System.Diagnostics.CodeAnalysis;
using Chasm.Utilities;

namespace SoRR
{
    /// <summary>
    ///   <para>Provides a set of static methods to manage asset managers and load assets.</para>
    /// </summary>
    public static class Assets
    {
        private static readonly StringKeyedDictionary<AssetManager> managers = new();

        /// <summary>
        ///   <para>Adds the specified asset <paramref name="manager"/> to the global registry under the specified <paramref name="prefix"/>.</para>
        /// </summary>
        /// <param name="manager">The asset manager to register under the specified <paramref name="prefix"/>.</param>
        /// <param name="prefix">The global prefix to register the specified asset <paramref name="manager"/> under.</param>
        public static void RegisterAssetManager(AssetManager manager, string prefix)
        {
            if (manager is null) throw new ArgumentNullException(nameof(manager));
            if (prefix is null) throw new ArgumentNullException(nameof(prefix));
            if (manager.registeredPrefix is not null)
                throw new ArgumentException("The specified manager already has a registered prefix.", nameof(manager));

            managers.Add(prefix, manager);
            manager.registeredPrefix = prefix;
        }
        /// <summary>
        ///   <para>Removes the specified asset <paramref name="manager"/> from the global registry.</para>
        /// </summary>
        /// <param name="manager">The asset manager to remove from the global registry.</param>
        /// <returns><see langword="true"/>, if the specified asset <paramref name="manager"/> was successfully removed; otherwise, <see langword="false"/>.</returns>
        public static bool UnRegisterAssetManager([NotNullWhen(true)] AssetManager? manager)
        {
            if (manager?.registeredPrefix is not null && managers.Remove(manager.registeredPrefix))
            {
                manager.registeredPrefix = null;
                return true;
            }
            return false;
        }

        /// <summary>
        ///   <para>Returns the handle for an asset at the specified <paramref name="fullPath"/>.</para>
        /// </summary>
        /// <param name="fullPath">A fully qualified path to the asset to get the handle of.</param>
        /// <returns>The handle for the specified asset, or <see langword="null"/> if it is not found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fullPath"/> is <see langword="null"/>.</exception>
        public static AssetHandle? GetHandle(string fullPath)
        {
            if (fullPath is null) throw new ArgumentNullException(nameof(fullPath));
            return GetHandle(fullPath.AsSpan());
        }
        /// <summary>
        ///   <para>Returns the handle for an asset at the specified <paramref name="fullPath"/>.</para>
        /// </summary>
        /// <param name="fullPath">A fully qualified path to the asset to get the handle of.</param>
        /// <returns>The handle for the specified asset, or <see langword="null"/> if it is not found.</returns>
        public static AssetHandle? GetHandle(ReadOnlySpan<char> fullPath)
        {
            SplitPath(fullPath, out var prefix, out var relativePath);
            return managers.TryGetValue(prefix, out AssetManager? manager)
                ? manager.GetHandle(relativePath)
                : null;
        }

        /// <summary>
        ///   <para>Loads an asset at the specified <paramref name="fullPath"/> and returns it.</para>
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="fullPath">A fully qualified path to the asset to load.</param>
        /// <returns>The specified asset.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fullPath"/> is <see langword="null"/>.</exception>
        /// <exception cref="AssetNotFoundException">An asset at the specified <paramref name="fullPath"/> could not be found.</exception>
        /// <exception cref="InvalidCastException">An asset at the specified <paramref name="fullPath"/> could not be cast to type <typeparamref name="T"/>.</exception>
        public static T? Load<T>(string fullPath)
        {
            if (fullPath is null) throw new ArgumentNullException(nameof(fullPath));
            return Load<T>(fullPath.AsSpan());
        }
        /// <summary>
        ///   <para>Loads an asset at the specified <paramref name="fullPath"/> and returns it.</para>
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="fullPath">A fully qualified path to the asset to load.</param>
        /// <returns>The specified asset.</returns>
        /// <exception cref="AssetNotFoundException">An asset at the specified <paramref name="fullPath"/> could not be found.</exception>
        /// <exception cref="InvalidCastException">An asset at the specified <paramref name="fullPath"/> could not be cast to type <typeparamref name="T"/>.</exception>
        public static T? Load<T>(ReadOnlySpan<char> fullPath)
        {
            SplitPath(fullPath, out var prefix, out var relativePath);
            return managers.TryGetValue(prefix, out AssetManager? manager)
                ? manager.Load<T>(relativePath)
                : throw new ArgumentException("Could not find specified asset manager prefix.", nameof(fullPath));
        }

        /// <summary>
        ///   <para>Tries to load an asset at the specified <paramref name="fullPath"/>, and returns a value indicating whether the operation was successful.</para>
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="fullPath">A fully qualified path to the asset to load.</param>
        /// <param name="asset">When this method returns, contains the specified asset, if it was successfully loaded, or <see langword="default"/> if it could not be loaded.</param>
        /// <returns><see langword="true"/>, if the specified asset was successfully loaded; otherwise, <see langword="false"/>.</returns>
        public static bool TryLoad<T>(string fullPath, [NotNullWhen(true)] out T? asset)
        {
            if (fullPath is null) return Util.Fail(out asset);
            return TryLoad(fullPath.AsSpan(), out asset);
        }
        /// <summary>
        ///   <para>Tries to load an asset at the specified <paramref name="fullPath"/>, and returns a value indicating whether the operation was successful.</para>
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="fullPath">A fully qualified path to the asset to load.</param>
        /// <param name="asset">When this method returns, contains the specified asset, if it was successfully loaded, or <see langword="default"/> if it could not be loaded.</param>
        /// <returns><see langword="true"/>, if the specified asset was successfully loaded; otherwise, <see langword="false"/>.</returns>
        public static bool TryLoad<T>(ReadOnlySpan<char> fullPath, [NotNullWhen(true)] out T? asset)
        {
            SplitPath(fullPath, out var prefix, out var relativePath);
            if (managers.TryGetValue(prefix, out AssetManager? manager))
                return manager.TryLoad(relativePath, out asset);
            asset = default;
            return false;
        }

        private static void SplitPath(ReadOnlySpan<char> query, out ReadOnlySpan<char> prefix, out ReadOnlySpan<char> path)
        {
            int separatorIndex = query.IndexOf(":/");
            if (separatorIndex == -1)
            {
                if (query.Length > 0 && query[0] == '/')
                    query = query[1..];
                // [ '/' ] <path>
                prefix = default;
                path = query;
                return;
            }
            // <prefix> ':/' <path>
            prefix = query[..separatorIndex];
            path = query[(separatorIndex + 2)..];
        }

    }
}
