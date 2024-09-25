using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace SoRR
{
    public abstract class AssetManager
    {
        internal string? registeredPrefix;
        protected readonly Dictionary<string, object> cache = [];

        public abstract string DisplayName { get; }

        // TODO: use ReadOnlySpan<char> in load methods, to avoid unnecessary allocations
        // TODO: a StringKeyedDictionary<TValue> would need to be implemented for that

        public bool TryLoad<T>(string path, [NotNullWhen(true)] out T? asset)
        {
            if (default(T) is not null) throw new NotSupportedException("TryLoad does not support struct assets.");
            return (asset = TryLoadAssetCore<T>(path, false)) is not null;
        }
        public T Load<T>(string path)
            => TryLoadAssetCore<T>(path, true)!;
        public T? LoadOrDefault<T>(string path)
            => TryLoadAssetCore<T>(path, false);

        private T? TryLoadAssetCore<T>(string path, bool throwOnError)
        {
            // Special case: Texture2D is loaded from a Sprite
            if (typeof(T) == typeof(Texture2D))
                return (T?)(object?)TryLoadAssetCore<Sprite>(path, throwOnError)?.texture;

            object? result = TryLoadAssetCore2(path);

            if (result is null)
            {
                if (!throwOnError) return default;
                throw new AssetNotFoundException(this, path);
            }
            if (result is not T asset)
            {
                if (!throwOnError) return default;
                throw new InvalidCastException($"Could not cast asset '{path}' of type {result.GetType()} to {typeof(T)}.");
            }

            return asset;
        }
        private object? TryLoadAssetCore2(string path)
        {
            // TODO: add warning logs here, instead of handling it like it's okay
            if (path.Contains('\\')) path = path.Replace('\\', '/');
            if (path.StartsWith('/')) path = path[1..];

            if (!cache.TryGetValue(path, out object? asset))
            {
                asset = LoadAssetHandler(path);
                if (asset is not null) cache.Add(path, asset);
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
