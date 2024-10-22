using System;
using System.Diagnostics.CodeAnalysis;
using Chasm.Utilities;
using UnityEngine;

namespace SoRR
{
    /// <summary>
    ///   <para>Represents an asset manager, that loads and caches various assets.</para>
    /// </summary>
    public abstract class AssetManager : IDisposable
    {
        internal string? registeredPrefix;
        /// <summary>
        ///   <para>The asset manager's cache of asset handles.</para>
        /// </summary>
        protected StringKeyedDictionary<AssetHandle> cache = new();
        /// <summary>
        ///   <para>Determines whether this asset manager has been disposed of.</para>
        /// </summary>
        protected bool disposed;

        /// <summary>
        ///   <para>Gets the asset manager's display name.</para>
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        ///   <para>Returns the string representation of this asset manager: its display name along with the registered prefix.</para>
        /// </summary>
        /// <returns>The string representation of this asset manager.</returns>
        public override string ToString()
            => registeredPrefix is null ? $"{DisplayName} (external)" : $"{DisplayName} ({registeredPrefix}:/*)";

        /// <summary>
        ///   <para>Releases all resources used by the asset manager.</para>
        /// </summary>
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        ///   <para>Releases the unmanaged resources used by the asset manager and optionally releases the managed resources.</para>
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Assets.UnRegisterAssetManager(this);
                cache = null!;
            }
        }

        /// <summary>
        ///   <para>Returns a new asset, loaded from the specified path, or <see langword="null"/> if the specified asset is not found.</para>
        /// </summary>
        /// <param name="assetPath">A relative path to the asset to load.</param>
        /// <returns>A new asset, loaded from the specified path, or <see langword="null"/> if it is not found.</returns>
        protected internal abstract object? LoadNewAssetOrNull(string assetPath);

        private AssetHandle? GetHandleCore(ReadOnlySpan<char> path, string? pathString)
        {
            Guard.ThrowIfDisposed(disposed, this);

            // TODO: add warning logs here, instead of handling it like it's okay
            if (path.IndexOf('\\') >= 0)
            {
                path = pathString = path.ToString().Replace('\\', '/');
            }
            if (path.Length > 0 && path[0] == '/')
            {
                path = path[1..];
                pathString = null;
            }

            if (!cache.TryGetValue(path, out AssetHandle? handle))
            {
                pathString ??= path.ToString();
                object? asset = null;
                try
                {
                    asset = LoadNewAssetOrNull(pathString);
                }
                catch (Exception ex)
                {
                    // TODO: Log the exception somewhere, but don't throw
                }
                if (asset is not null)
                    cache.Add(pathString, handle = new AssetHandle(this, pathString, asset));
            }
            return handle;
        }

        /// <summary>
        ///   <para>Returns the handle for an asset at the specified <paramref name="path"/>, or <see langword="null"/> if the specified asset is not found.</para>
        /// </summary>
        /// <param name="path">A relative path to the asset to get the handle of.</param>
        /// <returns>The handle for the specified asset, or <see langword="null"/> if it is not found.</returns>
        public AssetHandle? GetHandle(ReadOnlySpan<char> path)
            => GetHandleCore(path, null);

        private T? LoadAssetCore<T>(ReadOnlySpan<char> path, string? pathString, bool throwOnError)
        {
            // Special case: Texture2D is loaded from a Sprite
            if (typeof(T) == typeof(Texture2D))
                return (T?)(object?)LoadAssetCore<Sprite>(path, pathString, throwOnError)?.texture;

            // Get the associated handle, and try to get the asset using it
            object? result = GetHandleCore(path, pathString)?.Value;

            if (result is null)
            {
                if (!throwOnError) return default;
                throw new AssetNotFoundException(this, path.ToString());
            }
            if (result is not T asset)
            {
                if (!throwOnError) return default;
                throw new InvalidCastException($"Could not cast asset '{path.ToString()}' of type {result.GetType()} to {typeof(T)}.");
            }

            return asset;
        }

        /// <summary>
        ///   <para>Loads an asset at the specified <paramref name="path"/> and returns it.</para>
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="path">A relative path to the asset to load.</param>
        /// <returns>The specified asset.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="AssetNotFoundException">An asset at the specified <paramref name="path"/> could not be found.</exception>
        /// <exception cref="InvalidCastException">An asset at the specified <paramref name="path"/> could not be cast to type <typeparamref name="T"/>.</exception>
        public T? Load<T>(string path)
        {
            Guard.ThrowIfNull(path);
            return LoadAssetCore<T>(path, path, true);
        }
        /// <summary>
        ///   <para>Loads an asset at the specified <paramref name="path"/> and returns it.</para>
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="path">A relative path to the asset to load.</param>
        /// <returns>The specified asset.</returns>
        /// <exception cref="AssetNotFoundException">An asset at the specified <paramref name="path"/> could not be found.</exception>
        /// <exception cref="InvalidCastException">An asset at the specified <paramref name="path"/> could not be cast to type <typeparamref name="T"/>.</exception>
        public T? Load<T>(ReadOnlySpan<char> path)
            => LoadAssetCore<T>(path, null, true);

        /// <summary>
        ///   <para>Tries to load an asset at the specified <paramref name="path"/>, and returns a value indicating whether the operation was successful.</para>
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="path">A relative path to the asset to load.</param>
        /// <param name="asset">When this method returns, contains the specified asset, if it was successfully loaded, or <see langword="default"/> if it could not be loaded.</param>
        /// <returns><see langword="true"/>, if the specified asset was successfully loaded; otherwise, <see langword="false"/>.</returns>
        public bool TryLoad<T>(string path, [NotNullWhen(true)] out T? asset)
        {
            if (path is null) return Util.Fail(out asset);
            return (asset = LoadAssetCore<T>(path, path, false)) is not null;
        }
        /// <summary>
        ///   <para>Tries to load an asset at the specified <paramref name="path"/>, and returns a value indicating whether the operation was successful.</para>
        /// </summary>
        /// <typeparam name="T">The type of the asset to load.</typeparam>
        /// <param name="path">A relative path to the asset to load.</param>
        /// <param name="asset">When this method returns, contains the specified asset, if it was successfully loaded, or <see langword="default"/> if it could not be loaded.</param>
        /// <returns><see langword="true"/>, if the specified asset was successfully loaded; otherwise, <see langword="false"/>.</returns>
        public bool TryLoad<T>(ReadOnlySpan<char> path, [NotNullWhen(true)] out T? asset)
            => (asset = LoadAssetCore<T>(path, null, false)) is not null;

        /// <summary>
        ///   <para>Reloads an asset at the specified <paramref name="path"/>.</para>
        /// </summary>
        /// <param name="path">A relative path to the asset to reload.</param>
        protected void ReloadAssetPath(ReadOnlySpan<char> path)
        {
            if (cache.TryGetValue(path, out AssetHandle? handle))
                handle.TriggerReload();
        }

    }
}
