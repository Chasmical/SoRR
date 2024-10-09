using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace SoRR
{
    public abstract class AssetManager : IDisposable
    {
        internal string? registeredPrefix;
        protected StringKeyedDictionary<AssetHandle> cache = new();
        protected bool disposed;

        public abstract string DisplayName { get; }

        public override string ToString()
            => registeredPrefix is null ? $"{DisplayName} (external)" : $"{DisplayName} ({registeredPrefix}:/*)";

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Assets.UnRegisterAssetManager(this);
                cache = null!;
            }
        }

        protected internal abstract object? LoadNewAssetOrNull(string assetPath);

        private AssetHandle? GetHandleCore(ReadOnlySpan<char> path, string? pathString)
        {
            if (disposed) throw new ObjectDisposedException(ToString());

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

        public T? Load<T>(string path)
            => LoadAssetCore<T>(path, path, true);
        public T? Load<T>(ReadOnlySpan<char> path)
            => LoadAssetCore<T>(path, null, true);

        public bool TryLoad<T>(string path, [NotNullWhen(true)] out T? asset)
            => (asset = LoadAssetCore<T>(path, path, false)) is not null;
        public bool TryLoad<T>(ReadOnlySpan<char> path, [NotNullWhen(true)] out T? asset)
            => (asset = LoadAssetCore<T>(path, null, false)) is not null;

        protected void ReloadAssetPath(ReadOnlySpan<char> path)
        {
            if (cache.TryGetValue(path, out AssetHandle? handle))
                handle.TriggerReload();
        }

    }
}
