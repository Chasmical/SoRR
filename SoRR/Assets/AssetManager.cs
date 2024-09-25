using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace SoRR
{
    public abstract class AssetManager
    {
        internal string? registeredPrefix;
        protected readonly StringKeyedDictionary<object> cache = new();

        public abstract string DisplayName { get; }

        public bool TryLoad<T>(string path, [NotNullWhen(true)] out T? asset)
        {
            if (default(T) is not null) throw new NotSupportedException("TryLoad does not support struct assets.");
            return (asset = TryLoadAssetCore<T>(path, path, false)) is not null;
        }
        public T Load<T>(string path)
            => TryLoadAssetCore<T>(path, path, true)!;
        public T? LoadOrDefault<T>(string path)
            => TryLoadAssetCore<T>(path, path, false);

        public bool TryLoad<T>(ReadOnlySpan<char> path, [NotNullWhen(true)] out T? asset)
        {
            if (default(T) is not null) throw new NotSupportedException("TryLoad does not support struct assets.");
            return (asset = TryLoadAssetCore<T>(path, null, false)) is not null;
        }
        public T Load<T>(ReadOnlySpan<char> path)
            => TryLoadAssetCore<T>(path, null, true)!;
        public T? LoadOrDefault<T>(ReadOnlySpan<char> path)
            => TryLoadAssetCore<T>(path, null, false);

        private T? TryLoadAssetCore<T>(ReadOnlySpan<char> path, string? pathString, bool throwOnError)
        {
            // Special case: Texture2D is loaded from a Sprite
            if (typeof(T) == typeof(Texture2D))
                return (T?)(object?)TryLoadAssetCore<Sprite>(path, pathString, throwOnError)?.texture;

            object? result = TryLoadAssetCore2(path, pathString);

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

        private object? TryLoadAssetCore2(ReadOnlySpan<char> path, string? pathString)
        {
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

            if (!cache.TryGetValue(path, out object? asset))
            {
                pathString ??= path.ToString();
                asset = LoadAssetHandler(pathString);
                if (asset is not null) cache.Add(pathString, asset);
            }
            return asset;
        }

        protected abstract object? LoadAssetHandler(string path);

        protected bool RefreshAsset(string assetPath)
        {
            if (!cache.Remove(assetPath, out object? oldAsset)) return false;

            // TODO: Send an event to refresh this asset everywhere
            // TODO: Consumers will re-request and re-load the asset if needed

            // TODO: Note: to ensure smooth reloads, null assets shouldn't break consumers

            return true;
        }

    }
}
