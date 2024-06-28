using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoRR
{
    public abstract class AssetManager
    {
        private readonly Dictionary<string, object?> assets = [];

        public abstract string DisplayName { get; }

        protected abstract object? LoadAsset(string path);

        private object? ResolveAsset(string path)
        {
            if (path.Contains('\\')) path = path.Replace('\\', '/');
            if (path.StartsWith('/')) path = path[1..];

            if (!assets.TryGetValue(path, out object? asset))
                assets.Add(path, asset = LoadAsset(path));

            return asset;
        }
        private T? GetAssetCore<T>(string path, bool throwOnError) where T : notnull
        {
            // Special case: Texture2D is loaded from a Sprite
            if (typeof(T) == typeof(Texture2D))
                return (T?)(object?)GetAssetCore<Sprite>(path, throwOnError)?.texture;

            object? obj = ResolveAsset(path);

            if (obj is null)
            {
                if (!throwOnError) return default;
                throw new AssetNotFoundException(this, path);
            }
            if (obj is not T tAsset)
            {
                if (!throwOnError) return default;
                throw new InvalidCastException($"Could not cast asset '{path}' of type {obj.GetType()} to {typeof(T)}.");
            }

            return tAsset;
        }

        public bool TryLoad<T>(string path, out T? asset) where T : notnull
        {
            if (default(T) is not null) throw new NotSupportedException("TryLoad does not support struct assets.");
            return (asset = GetAssetCore<T>(path, false)) is not null;
        }
        public T Load<T>(string path) where T : notnull
            => GetAssetCore<T>(path, true)!;

    }
}
