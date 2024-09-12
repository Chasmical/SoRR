using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoRR
{
    public abstract class AssetManager
    {
        protected readonly Dictionary<string, object> cache = [];

        public abstract string DisplayName { get; }

        protected abstract object? LoadAsset(string path);

        private object? FindInCacheOrLoadAsset(string path)
        {
            // TODO: add warning logs here, instead of handling it like it's okay
            if (path.Contains('\\')) path = path.Replace('\\', '/');
            if (path.StartsWith('/')) path = path[1..];

            if (!cache.TryGetValue(path, out object? asset))
            {
                asset = LoadAsset(path);
                if (asset is not null) cache.Add(path, asset);
            }
            return asset;
        }

        private T? TryLoadAssetCore<T>(string path, bool throwOnError)
        {
            // Special case: Texture2D is loaded from a Sprite
            if (typeof(T) == typeof(Texture2D))
                return (T?)(object?)TryLoadAssetCore<Sprite>(path, throwOnError)?.texture;

            object? result = FindInCacheOrLoadAsset(path);

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

        public bool TryLoad<T>(string path, out T? asset)
        {
            if (default(T) is not null) throw new NotSupportedException("TryLoad does not support struct assets.");
            return (asset = TryLoadAssetCore<T>(path, false)) is not null;
        }
        public T Load<T>(string path)
            => TryLoadAssetCore<T>(path, true)!;

    }
}
